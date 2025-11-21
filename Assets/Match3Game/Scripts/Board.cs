using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    wait,
    move
}

public enum PowerUpState
{
    None,
    ColorDestroy,
    SwapRandom,
    ColorChange,
    DestroyDot,
    DestroyColumn,
    DestroyRow,
    DestroyCross,
    Destroy3x3,
    SwapColumn,
    SwapRow,
    ShuffleBoard
}

public enum TileKind
{
    Breakable,
    Blank,
    Normal
}

[System.Serializable]
public class TileType
{
    public int x;
    public int y;
    public TileKind tileKind;
}

public class Board : MonoBehaviour
{
    public GameState currentState = GameState.move;
    public PowerUpState currentPowerUp = PowerUpState.None;
    private string selectedColor = "";  // Để lưu màu được chọn cho color destroy/change
    private GameObject firstSelectedForSwap = null;  // Để lưu dot đầu tiên cho swap random
    public int width;
    public int height;
    public int offSet;

    public GameObject tilePrefab;
    public GameObject breakableTilePrefab;
    public GameObject[] dots;
    public GameObject destroyParticle;
    public TileType[] boardLayout;

    private bool[,] blankSpaces;
    private BackgroundTile[,] breakableTiles;
    public GameObject[,] allDots;
    public Dot currentDot;
    public FindMatches findMatches;

    [Header("Advanced Function")]
    //Score
    public int basePieceValue = 20;
    private int streakValue = 1;
    private ScoreManager scoreManager;

    //Sound
    private SoundManager soundManager;

    //Refresh Board is not conclude in
    public float refillDelay = 0.5f;

    public int[] scoreGoals;

    //Touch event
    [SerializeField]
    private GameObject soundParticle; // Hiệu ứng hạt
    [SerializeField]
    private AudioClip touchSound; // Âm thanh khi chạm
    [SerializeField]
    private AudioClip swipeSound; // Âm thanh khi vuốt
    [SerializeField]
    private AudioSource audioSource; // Nguồn phát âm thanh

    private GameObject firstSelectedObject; // Lưu đối tượng được chọn đầu tiên
    private Vector2 startTouchPosition; // Vị trí bắt đầu vuốt
    private Vector2 endTouchPosition; // Vị trí kết thúc vuốt
    private float minSwipeDistance = 0.5f; // Khoảng cách tối thiểu để được coi là vuốt

    private GameObject firstSelectedForColumnSwap = null;
    private GameObject firstSelectedForRowSwap = null;

    // Use this for initialization
    void Start()
    {
        soundManager = FindObjectOfType<SoundManager>();
        scoreManager = FindObjectOfType<ScoreManager>();
        breakableTiles = new BackgroundTile[width, height];
        findMatches = FindObjectOfType<FindMatches>();
        blankSpaces = new bool[width, height];
        allDots = new GameObject[width, height];

        //Convert AudioClip to AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        SetUp();
    }

    private void Update()
    {
        CheckSelectedObject();
        CheckSwipeGesture();
    }

    public void GenerateBlankSpaces()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tileKind == TileKind.Blank)
            {
                blankSpaces[boardLayout[i].x, boardLayout[i].y] = true;
            }
        }
    }

    public void GenerateBreakableTiles()
    {
        //Look at all the tiles in the layout
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //if a tile is a "Jelly" tile
            if (boardLayout[i].tileKind == TileKind.Breakable)
            {
                //Create a "Jelly" tile at that position;
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(breakableTilePrefab, tempPosition, Quaternion.identity);
                breakableTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void SetUp()
    {
        GenerateBlankSpaces();
        GenerateBreakableTiles();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!blankSpaces[i, j])
                {
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    Vector2 tilePosition = new Vector2(i, j);
                    GameObject backgroundTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
                    backgroundTile.transform.parent = this.transform;
                    backgroundTile.name = "( " + i + ", " + j + " )";

                    int dotToUse = Random.Range(0, dots.Length);

                    int maxIterations = 0;

                    while (MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
                    {
                        dotToUse = Random.Range(0, dots.Length);
                        maxIterations++;
                        Debug.Log(maxIterations);
                    }
                    maxIterations = 0;

                    GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    dot.GetComponent<Dot>().row = j;
                    dot.GetComponent<Dot>().column = i;
                    dot.transform.parent = this.transform;
                    dot.name = "( " + i + ", " + j + " )";
                    allDots[i, j] = dot;
                }
            }

        }
    }

    private bool MatchesAt(int column, int row, GameObject piece)
    {
        if (column > 1 && row > 1)
        {
            if (allDots[column - 1, row] != null && allDots[column - 2, row] != null)
            {
                if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
                {
                    return true;
                }
            }
            if (allDots[column, row - 1] != null && allDots[column, row - 2] != null)
            {
                if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }

        }
        else if (column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allDots[column, row - 1] != null && allDots[column, row - 2] != null)
                {
                    if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
                    {
                        return true;
                    }
                }
            }
            if (column > 1)
            {
                if (allDots[column - 1, row] != null && allDots[column - 2, row] != null)
                {
                    if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }


    private bool ColumnOrRow()
    {
        int numberHorizontal = 0;
        int numberVertical = 0;
        Dot firstPiece = findMatches.currentMatches[0].GetComponent<Dot>();
        if (firstPiece != null)
        {
            foreach (GameObject currentPiece in findMatches.currentMatches)
            {
                Dot dot = currentPiece.GetComponent<Dot>();
                if (dot.row == firstPiece.row)
                {
                    numberHorizontal++;
                }
                if (dot.column == firstPiece.column)
                {
                    numberVertical++;
                }
            }
        }
        return (numberVertical == 5 || numberHorizontal == 5);

    }

    private void CheckToMakeBombs()
    {
        if (findMatches.currentMatches.Count == 4 || findMatches.currentMatches.Count == 7)
        {
            findMatches.CheckBombs();
        }
        if (findMatches.currentMatches.Count == 5 || findMatches.currentMatches.Count == 8)
        {
            if (ColumnOrRow())
            {
                //Make a color bomb
                //is the current dot matched?
                if (currentDot != null)
                {
                    if (currentDot.isMatched)
                    {
                        if (!currentDot.isColorBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeColorBomb();
                        }
                    }
                    else
                    {
                        if (currentDot.otherDot != null)
                        {
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if (otherDot.isMatched)
                            {
                                if (!otherDot.isColorBomb)
                                {
                                    otherDot.isMatched = false;
                                    otherDot.MakeColorBomb();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //Make a adjacent bomb
                //is the current dot matched?
                if (currentDot != null)
                {
                    if (currentDot.isMatched)
                    {
                        if (!currentDot.isAdjacentBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeAdjacentBomb();
                        }
                    }
                    else
                    {
                        if (currentDot.otherDot != null)
                        {
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if (otherDot.isMatched)
                            {
                                if (!otherDot.isAdjacentBomb)
                                {
                                    otherDot.isMatched = false;
                                    otherDot.MakeAdjacentBomb();
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void DestroyMatchesAt(int column, int row)
    {
        if (allDots[column, row].GetComponent<Dot>().isMatched)
        {
            //How many elements are in the matched pieces list from findmatches?
            if (findMatches.currentMatches.Count >= 4)
            {
                CheckToMakeBombs();
            }

            //Does a tile need to break?
            if (breakableTiles[column, row] != null)
            {
                //if it does, give one damage.
                breakableTiles[column, row].TakeDamage(1);
                if (breakableTiles[column, row].hitPoints <= 0)
                {
                    breakableTiles[column, row] = null;
                }

            }

            //Does the sound manager exist?
            if (soundManager != null)
            {
                soundManager.DestroyTileSound();
            }

            GameObject particle = Instantiate(destroyParticle,
                                              allDots[column, row].transform.position,
                                              Quaternion.identity);
            Destroy(particle, .5f);
            Destroy(allDots[column, row]);
            scoreManager.IncreaseScore(basePieceValue * streakValue);
            allDots[column, row] = null;
        }
    }

    public void DestroyMatches()
    {
        // Chuyển game state sang wait khi bắt đầu destroy
        currentState = GameState.wait;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        findMatches.currentMatches.Clear();
        StartCoroutine(DecreaseRowCo2());
    }

    private IEnumerator DecreaseRowCo2()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!blankSpaces[i, j] && allDots[i, j] == null)
                {
                    for (int k = j + 1; k < height; k++)
                    {
                        if (allDots[i, k] != null)
                        {
                            allDots[i, k].GetComponent<Dot>().row = j;
                            allDots[i, k] = null;
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(refillDelay * 0.5f);
        StartCoroutine(FillBoardCo());

        // Chuyển game state về move sau khi hoàn tất
        currentState = GameState.move;
    }

    private void RefillBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null && !blankSpaces[i, j])
                {
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    int dotToUse = Random.Range(0, dots.Length);
                    int maxIterations = 0;

                    while (MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
                    {
                        maxIterations++;
                        dotToUse = Random.Range(0, dots.Length);
                    }

                    maxIterations = 0;
                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    allDots[i, j] = piece;
                    piece.GetComponent<Dot>().row = j;
                    piece.GetComponent<Dot>().column = i;

                }
            }
        }
    }

    private bool MatchesOnBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (allDots[i, j].GetComponent<Dot>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoardCo()
    {
        RefillBoard();
        yield return new WaitForSeconds(refillDelay);

        while (MatchesOnBoard())
        {
            streakValue++;
            DestroyMatches();
            yield return new WaitForSeconds(2 * refillDelay);

        }
        findMatches.currentMatches.Clear();
        currentDot = null;


        if (IsDeadlocked())
        {
            StartCoroutine(ShuffleBoardCoroutine());
            Debug.Log("Deadlocked!!!");
        }
        yield return new WaitForSeconds(refillDelay);
        currentState = GameState.move;
        streakValue = 1;

    }

    private void SwitchPieces(int column, int row, Vector2 direction)
    {
        //Take the second piece and save it in a holder
        GameObject holder = allDots[column + (int)direction.x, row + (int)direction.y] as GameObject;
        //switching the first dot to be the second position
        allDots[column + (int)direction.x, row + (int)direction.y] = allDots[column, row];
        //Set the first dot to be the second dot
        allDots[column, row] = holder;
    }

    private bool CheckForMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    //Make sure that one and two to the right are in the
                    //board
                    if (i < width - 2)
                    {
                        //Check if the dots to the right and two to the right exist
                        if (allDots[i + 1, j] != null && allDots[i + 2, j] != null)
                        {
                            if (allDots[i + 1, j].tag == allDots[i, j].tag
                               && allDots[i + 2, j].tag == allDots[i, j].tag)
                            {
                                return true;
                            }
                        }

                    }
                    if (j < height - 2)
                    {
                        //Check if the dots above exist
                        if (allDots[i, j + 1] != null && allDots[i, j + 2] != null)
                        {
                            if (allDots[i, j + 1].tag == allDots[i, j].tag
                               && allDots[i, j + 2].tag == allDots[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool SwitchAndCheck(int column, int row, Vector2 direction)
    {
        SwitchPieces(column, row, direction);
        if (CheckForMatches())
        {
            SwitchPieces(column, row, direction);
            return true;
        }
        SwitchPieces(column, row, direction);
        return false;
    }

    private bool IsDeadlocked()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (i < width - 1)
                    {
                        if (SwitchAndCheck(i, j, Vector2.right))
                        {
                            return false;
                        }
                    }
                    if (j < height - 1)
                    {
                        if (SwitchAndCheck(i, j, Vector2.up))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    private IEnumerator ShuffleBoardCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        //Create a list of game objects
        List<GameObject> newBoard = new List<GameObject>();
        //Add every piece to this list
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    newBoard.Add(allDots[i, j]);
                }
            }
        }
        yield return new WaitForSeconds(0.5f);
        //for every spot on the board. . . 
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //if this spot shouldn't be blank
                if (!blankSpaces[i, j])
                {
                    //Pick a random number
                    int pieceToUse = Random.Range(0, newBoard.Count);

                    //Assign the column to the piece
                    int maxIterations = 0;

                    while (MatchesAt(i, j, newBoard[pieceToUse]) && maxIterations < 100)
                    {
                        pieceToUse = Random.Range(0, newBoard.Count);
                        maxIterations++;
                        Debug.Log(maxIterations);
                    }

                    //Make a container for the piece
                    Dot piece = newBoard[pieceToUse].GetComponent<Dot>();
                    maxIterations = 0;
                    piece.column = i;
                    //Assign the row to the piece
                    piece.row = j;
                    //Fill in the dots array with this new piece
                    allDots[i, j] = newBoard[pieceToUse];
                    //Remove it from the list
                    newBoard.Remove(newBoard[pieceToUse]);
                }
            }
        }
        //Check if it's still deadlocked
        if (IsDeadlocked())
        {
            StartCoroutine(ShuffleBoardCoroutine());
        }
    }

    public void ShuffleBoard()
    {
        if (currentState != GameState.move) return;
        currentState = GameState.wait;

        //Create a list of game objects
        List<GameObject> newBoard = new List<GameObject>();
        //Add every piece to this list
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    newBoard.Add(allDots[i, j]);
                }
            }
        }

        //for every spot on the board. . . 
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //if this spot shouldn't be blank
                if (!blankSpaces[i, j])
                {
                    //Pick a random number
                    int pieceToUse = Random.Range(0, newBoard.Count);

                    //Assign the column to the piece
                    int maxIterations = 0;

                    while (MatchesAt(i, j, newBoard[pieceToUse]) && maxIterations < 100)
                    {
                        pieceToUse = Random.Range(0, newBoard.Count);
                        maxIterations++;
                    }

                    //Make a container for the piece
                    Dot piece = newBoard[pieceToUse].GetComponent<Dot>();
                    maxIterations = 0;
                    piece.column = i;
                    //Assign the row to the piece
                    piece.row = j;
                    //Fill in the dots array with this new piece
                    allDots[i, j] = newBoard[pieceToUse];
                    //Remove it from the list
                    newBoard.Remove(newBoard[pieceToUse]);
                }
            }
        }

        // Check for matches after shuffle
        findMatches.FindAllMatches();
        if (findMatches.currentMatches.Count > 0)
        {
            DestroyMatches();
        }
        else
        {
            //Check if it's still deadlocked
            if (IsDeadlocked())
            {
                ShuffleBoard();
            }
            else
            {
                currentState = GameState.move;
            }
        }
    }

    /*Check touch event
     If tile is selected it will glow
    */
    // Hàm kiểm tra chọn vật thể
    private void CheckSelectedObject()
    {
        if (Input.GetMouseButtonDown(0) && (currentState == GameState.move || currentPowerUp != PowerUpState.None))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null)
            {
                GameObject selectedObject = hit.collider.gameObject;
                Dot selectedDot = selectedObject.GetComponent<Dot>();

                if (selectedDot != null && allDots[selectedDot.column, selectedDot.row] == selectedObject)
                {
                    // Nếu đang có power-up active
                    if (currentPowerUp != PowerUpState.None)
                    {
                        HandlePowerUpSelection(selectedObject);
                        return;
                    }

                    // Normal dot selection logic
                    if (currentDot == null)
                    {
                        selectedDot.SelectDot();
                        if (audioSource != null && touchSound != null)
                        {
                            audioSource.PlayOneShot(touchSound);
                        }
                    }
                    else
                    {
                        // Nếu chọn lại chính dot đã chọn
                        if (selectedDot == currentDot)
                        {
                            selectedDot.DeselectDot();
                            return;
                        }

                        // Kiểm tra xem có phải dot kề nhau không
                        if (Mathf.Abs(selectedDot.column - currentDot.column) + Mathf.Abs(selectedDot.row - currentDot.row) == 1)
                        {
                            currentState = GameState.wait;
                            selectedDot.SwapDots();
                            if (audioSource != null && swipeSound != null)
                            {
                                audioSource.PlayOneShot(swipeSound);
                            }
                        }
                        else
                        {
                            currentDot.DeselectDot();
                            selectedDot.SelectDot();
                        }
                    }
                }
            }
            else if (currentDot != null && currentPowerUp == PowerUpState.None)
            {
                currentDot.DeselectDot();
            }
        }
    }

    private void CheckSwipeGesture()
    {
        if (currentState != GameState.move)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            endTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (Vector2.Distance(startTouchPosition, endTouchPosition) >= minSwipeDistance)
            {
                if (audioSource != null && swipeSound != null)
                {
                    audioSource.PlayOneShot(swipeSound);
                }
            }
        }
    }

    public void DestroyAllDots()
    {
        // Chuyển game state sang wait
        currentState = GameState.wait;

        // Tạo list chứa tất cả các dot
        List<GameObject> allDotsToDestroy = new List<GameObject>();

        // Thêm tất cả dot vào list
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    allDotsToDestroy.Add(allDots[i, j]);
                    // Tạo hiệu ứng nổ
                    GameObject particle = Instantiate(destroyParticle,
                        allDots[i, j].transform.position,
                        Quaternion.identity);
                    Destroy(particle, .5f);
                    // Tăng điểm
                    scoreManager.IncreaseScore(basePieceValue);
                    // Phát âm thanh
                    if (soundManager != null)
                    {
                        soundManager.DestroyTileSound();
                    }
                    // Xóa dot khỏi mảng
                    allDots[i, j] = null;
                }
            }
        }

        // Hủy tất cả các dot
        foreach (GameObject dot in allDotsToDestroy)
        {
            Destroy(dot);
        }

        // Bắt đầu điền lại bảng
        StartCoroutine(DecreaseRowCo2());
    }

    public void DestroyRandomDots(int count)
    {
        if (currentState != GameState.move) return;

        currentState = GameState.wait;
        List<GameObject> availableDots = new List<GameObject>();

        // Collect all available dots
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    availableDots.Add(allDots[i, j]);
                }
            }
        }

        // Randomly select and destroy dots
        for (int i = 0; i < Mathf.Min(count, availableDots.Count); i++)
        {
            int randomIndex = Random.Range(0, availableDots.Count);
            GameObject dotToDestroy = availableDots[randomIndex];

            // Get position
            Dot dot = dotToDestroy.GetComponent<Dot>();
            int column = dot.column;
            int row = dot.row;

            // Create effect
            GameObject particle = Instantiate(destroyParticle, dotToDestroy.transform.position, Quaternion.identity);
            Destroy(particle, .5f);

            // Add score and play sound
            scoreManager.IncreaseScore(basePieceValue);
            if (soundManager != null)
            {
                soundManager.DestroyTileSound();
            }

            // Remove from arrays
            allDots[column, row] = null;
            Destroy(dotToDestroy);
            availableDots.RemoveAt(randomIndex);
        }

        StartCoroutine(DecreaseRowCo2());
    }

    public void ActivateColorDestroy()
    {
        if (currentState != GameState.move) return;
        currentPowerUp = PowerUpState.ColorDestroy;
    }

    public void ActivateSwapRandom()
    {
        if (currentState != GameState.move) return;
        currentPowerUp = PowerUpState.SwapRandom;
        firstSelectedForSwap = null;
    }

    public void ActivateColorChange()
    {
        if (currentState != GameState.move) return;
        currentPowerUp = PowerUpState.ColorChange;
        selectedColor = "";
    }

    private void HandlePowerUpSelection(GameObject selectedObject)
    {
        if (currentPowerUp == PowerUpState.None) return;

        // Đảm bảo bỏ chọn dot hiện tại nếu có
        if (currentDot != null)
        {
            currentDot.GetComponent<Dot>().DeselectDot();
        }

        switch (currentPowerUp)
        {
            case PowerUpState.ColorDestroy:
                DestroyColor(selectedObject.tag);
                break;

            case PowerUpState.SwapRandom:
                HandleSwapRandom(selectedObject);
                break;

            case PowerUpState.ColorChange:
                HandleColorChange(selectedObject);
                break;

            case PowerUpState.DestroyDot:
                DestroyDot(selectedObject);
                break;

            case PowerUpState.DestroyColumn:
                DestroyColumn(selectedObject);
                break;

            case PowerUpState.DestroyRow:
                DestroyRow(selectedObject);
                break;

            case PowerUpState.DestroyCross:
                DestroyCross(selectedObject);
                break;

            case PowerUpState.Destroy3x3:
                Destroy3x3(selectedObject);
                break;

            case PowerUpState.SwapColumn:
                HandleSwapColumn(selectedObject);
                break;

            case PowerUpState.SwapRow:
                HandleSwapRow(selectedObject);
                break;
        }
    }

    private void DestroyColor(string color)
    {
        currentState = GameState.wait;
        List<GameObject> dotsToDestroy = new List<GameObject>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null && allDots[i, j].tag == color)
                {
                    dotsToDestroy.Add(allDots[i, j]);
                    GameObject particle = Instantiate(destroyParticle, allDots[i, j].transform.position, Quaternion.identity);
                    Destroy(particle, .5f);
                    scoreManager.IncreaseScore(basePieceValue);
                    if (soundManager != null)
                    {
                        soundManager.DestroyTileSound();
                    }
                    allDots[i, j] = null;
                }
            }
        }

        foreach (GameObject dot in dotsToDestroy)
        {
            Destroy(dot);
        }

        currentPowerUp = PowerUpState.None;
        StartCoroutine(DecreaseRowCo2());
    }

    private void HandleSwapRandom(GameObject selectedObject)
    {
        if (firstSelectedForSwap == null)
        {
            firstSelectedForSwap = selectedObject;
            selectedObject.GetComponent<Dot>().SelectDot();
        }
        else
        {
            // Swap the positions
            Dot firstDot = firstSelectedForSwap.GetComponent<Dot>();
            Dot secondDot = selectedObject.GetComponent<Dot>();

            // Store positions
            int tempColumn = firstDot.column;
            int tempRow = firstDot.row;

            // Update positions
            firstDot.column = secondDot.column;
            firstDot.row = secondDot.row;
            secondDot.column = tempColumn;
            secondDot.row = tempRow;

            // Update array
            allDots[firstDot.column, firstDot.row] = firstSelectedForSwap;
            allDots[secondDot.column, secondDot.row] = selectedObject;

            // Reset highlight
            firstDot.DeselectDot();
            secondDot.DeselectDot();

            // Check for matches
            findMatches.FindAllMatches();
            if (findMatches.currentMatches.Count > 0)
            {
                DestroyMatches();
            }

            firstSelectedForSwap = null;
            currentPowerUp = PowerUpState.None;
        }
    }

    private void HandleColorChange(GameObject selectedObject)
    {
        if (selectedColor == "")
        {
            selectedColor = selectedObject.tag;
            // Highlight all dots of the selected color
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (allDots[i, j] != null && allDots[i, j].tag == selectedColor)
                    {
                        allDots[i, j].GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
                    }
                }
            }
        }
        else
        {
            string newColor = selectedObject.tag;
            if (newColor != selectedColor)
            {
                // Change all dots of selectedColor to newColor
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (allDots[i, j] != null && allDots[i, j].tag == selectedColor)
                        {
                            // Create new dot of newColor
                            Vector2 position = allDots[i, j].transform.position;
                            Destroy(allDots[i, j]);

                            GameObject newDot = null;
                            foreach (GameObject dot in dots)
                            {
                                if (dot.tag == newColor)
                                {
                                    newDot = Instantiate(dot, position, Quaternion.identity);
                                    break;
                                }
                            }

                            if (newDot != null)
                            {
                                allDots[i, j] = newDot;
                                newDot.GetComponent<Dot>().column = i;
                                newDot.GetComponent<Dot>().row = j;
                            }
                        }
                    }
                }

                // Check for matches after color change
                findMatches.FindAllMatches();
                if (findMatches.currentMatches.Count > 0)
                {
                    DestroyMatches();
                }
            }

            // Reset power-up state
            selectedColor = "";
            currentPowerUp = PowerUpState.None;
        }
    }

    public void ActivateDestroyDot()
    {
        if (currentState != GameState.move) return;
        currentPowerUp = PowerUpState.DestroyDot;
    }

    public void ActivateDestroyColumn()
    {
        if (currentState != GameState.move) return;
        currentPowerUp = PowerUpState.DestroyColumn;
    }

    public void ActivateDestroyRow()
    {
        if (currentState != GameState.move) return;
        currentPowerUp = PowerUpState.DestroyRow;
    }

    public void ActivateDestroyCross()
    {
        if (currentState != GameState.move) return;
        currentPowerUp = PowerUpState.DestroyCross;
    }

    public void ActivateDestroy3x3()
    {
        if (currentState != GameState.move) return;
        currentPowerUp = PowerUpState.Destroy3x3;
    }

    public void ActivateSwapColumn()
    {
        if (currentState != GameState.move) return;
        currentPowerUp = PowerUpState.SwapColumn;
        firstSelectedForColumnSwap = null;
    }

    public void ActivateSwapRow()
    {
        if (currentState != GameState.move) return;
        currentPowerUp = PowerUpState.SwapRow;
        firstSelectedForRowSwap = null;
    }

    private void DestroyDot(GameObject dot)
    {
        currentState = GameState.wait;
        Dot selectedDot = dot.GetComponent<Dot>();
        int column = selectedDot.column;
        int row = selectedDot.row;

        // Create effect
        GameObject particle = Instantiate(destroyParticle, dot.transform.position, Quaternion.identity);
        Destroy(particle, .5f);

        // Add score and play sound
        scoreManager.IncreaseScore(basePieceValue);
        if (soundManager != null)
        {
            soundManager.DestroyTileSound();
        }

        // Remove dot
        allDots[column, row] = null;
        Destroy(dot);

        currentPowerUp = PowerUpState.None;
        StartCoroutine(DecreaseRowCo2());
    }

    private void DestroyColumn(GameObject dot)
    {
        currentState = GameState.wait;
        Dot selectedDot = dot.GetComponent<Dot>();
        int column = selectedDot.column;

        for (int i = 0; i < height; i++)
        {
            if (allDots[column, i] != null)
            {
                GameObject particle = Instantiate(destroyParticle, allDots[column, i].transform.position, Quaternion.identity);
                Destroy(particle, .5f);
                scoreManager.IncreaseScore(basePieceValue);
                if (soundManager != null)
                {
                    soundManager.DestroyTileSound();
                }
                Destroy(allDots[column, i]);
                allDots[column, i] = null;
            }
        }

        currentPowerUp = PowerUpState.None;
        StartCoroutine(DecreaseRowCo2());
    }

    private void DestroyRow(GameObject dot)
    {
        currentState = GameState.wait;
        Dot selectedDot = dot.GetComponent<Dot>();
        int row = selectedDot.row;

        for (int i = 0; i < width; i++)
        {
            if (allDots[i, row] != null)
            {
                GameObject particle = Instantiate(destroyParticle, allDots[i, row].transform.position, Quaternion.identity);
                Destroy(particle, .5f);
                scoreManager.IncreaseScore(basePieceValue);
                if (soundManager != null)
                {
                    soundManager.DestroyTileSound();
                }
                Destroy(allDots[i, row]);
                allDots[i, row] = null;
            }
        }

        currentPowerUp = PowerUpState.None;
        StartCoroutine(DecreaseRowCo2());
    }

    private void DestroyCross(GameObject dot)
    {
        currentState = GameState.wait;
        Dot selectedDot = dot.GetComponent<Dot>();
        int column = selectedDot.column;
        int row = selectedDot.row;

        // Destroy column
        for (int i = 0; i < height; i++)
        {
            if (allDots[column, i] != null)
            {
                GameObject particle = Instantiate(destroyParticle, allDots[column, i].transform.position, Quaternion.identity);
                Destroy(particle, .5f);
                scoreManager.IncreaseScore(basePieceValue);
                if (soundManager != null)
                {
                    soundManager.DestroyTileSound();
                }
                Destroy(allDots[column, i]);
                allDots[column, i] = null;
            }
        }

        // Destroy row
        for (int i = 0; i < width; i++)
        {
            if (allDots[i, row] != null)
            {
                GameObject particle = Instantiate(destroyParticle, allDots[i, row].transform.position, Quaternion.identity);
                Destroy(particle, .5f);
                scoreManager.IncreaseScore(basePieceValue);
                if (soundManager != null)
                {
                    soundManager.DestroyTileSound();
                }
                Destroy(allDots[i, row]);
                allDots[i, row] = null;
            }
        }

        currentPowerUp = PowerUpState.None;
        StartCoroutine(DecreaseRowCo2());
    }

    private void Destroy3x3(GameObject dot)
    {
        currentState = GameState.wait;
        Dot selectedDot = dot.GetComponent<Dot>();
        int centerColumn = selectedDot.column;
        int centerRow = selectedDot.row;

        for (int i = centerColumn - 1; i <= centerColumn + 1; i++)
        {
            for (int j = centerRow - 1; j <= centerRow + 1; j++)
            {
                if (i >= 0 && i < width && j >= 0 && j < height && allDots[i, j] != null)
                {
                    GameObject particle = Instantiate(destroyParticle, allDots[i, j].transform.position, Quaternion.identity);
                    Destroy(particle, .5f);
                    scoreManager.IncreaseScore(basePieceValue);
                    if (soundManager != null)
                    {
                        soundManager.DestroyTileSound();
                    }
                    Destroy(allDots[i, j]);
                    allDots[i, j] = null;
                }
            }
        }

        currentPowerUp = PowerUpState.None;
        StartCoroutine(DecreaseRowCo2());
    }

    private void HandleSwapColumn(GameObject selectedObject)
    {
        if (firstSelectedForColumnSwap == null)
        {
            firstSelectedForColumnSwap = selectedObject;
            selectedObject.GetComponent<Dot>().SelectDot();
        }
        else
        {
            currentState = GameState.wait;
            Dot firstDot = firstSelectedForColumnSwap.GetComponent<Dot>();
            Dot secondDot = selectedObject.GetComponent<Dot>();
            int column1 = firstDot.column;
            int column2 = secondDot.column;

            // Swap entire columns
            for (int i = 0; i < height; i++)
            {
                GameObject temp = allDots[column1, i];
                allDots[column1, i] = allDots[column2, i];
                allDots[column2, i] = temp;

                if (allDots[column1, i] != null)
                {
                    allDots[column1, i].GetComponent<Dot>().column = column1;
                }
                if (allDots[column2, i] != null)
                {
                    allDots[column2, i].GetComponent<Dot>().column = column2;
                }
            }

            // Reset highlight
            firstDot.DeselectDot();
            secondDot.DeselectDot();

            // Check for matches
            findMatches.FindAllMatches();
            if (findMatches.currentMatches.Count > 0)
            {
                DestroyMatches();
            }
            else
            {
                currentState = GameState.move;
            }

            firstSelectedForColumnSwap = null;
            currentPowerUp = PowerUpState.None;
        }
    }

    private void HandleSwapRow(GameObject selectedObject)
    {
        if (firstSelectedForRowSwap == null)
        {
            firstSelectedForRowSwap = selectedObject;
            selectedObject.GetComponent<Dot>().SelectDot();
        }
        else
        {
            currentState = GameState.wait;
            Dot firstDot = firstSelectedForRowSwap.GetComponent<Dot>();
            Dot secondDot = selectedObject.GetComponent<Dot>();
            int row1 = firstDot.row;
            int row2 = secondDot.row;

            // Swap entire rows
            for (int i = 0; i < width; i++)
            {
                GameObject temp = allDots[i, row1];
                allDots[i, row1] = allDots[i, row2];
                allDots[i, row2] = temp;

                if (allDots[i, row1] != null)
                {
                    allDots[i, row1].GetComponent<Dot>().row = row1;
                }
                if (allDots[i, row2] != null)
                {
                    allDots[i, row2].GetComponent<Dot>().row = row2;
                }
            }

            // Reset highlight
            firstDot.DeselectDot();
            secondDot.DeselectDot();

            // Check for matches
            findMatches.FindAllMatches();
            if (findMatches.currentMatches.Count > 0)
            {
                DestroyMatches();
            }
            else
            {
                currentState = GameState.move;
            }

            firstSelectedForRowSwap = null;
            currentPowerUp = PowerUpState.None;
        }
    }

    public void ActivateShuffleBoard()
    {
        if (currentState != GameState.move) return;
        StartCoroutine(ShuffleBoardCoroutine());
    }

    public void CancelCurrentPowerUp()
    {
        // Đảm bảo bỏ chọn dot hiện tại nếu có
        if (currentDot != null)
        {
            currentDot.GetComponent<Dot>().DeselectDot();
        }

        // Reset các biến liên quan đến power-up
        if (firstSelectedForSwap != null)
        {
            firstSelectedForSwap.GetComponent<Dot>().DeselectDot();
            firstSelectedForSwap = null;
        }
        if (firstSelectedForColumnSwap != null)
        {
            firstSelectedForColumnSwap.GetComponent<Dot>().DeselectDot();
            firstSelectedForColumnSwap = null;
        }
        if (firstSelectedForRowSwap != null)
        {
            firstSelectedForRowSwap.GetComponent<Dot>().DeselectDot();
            firstSelectedForRowSwap = null;
        }

        // Reset power-up state
        currentPowerUp = PowerUpState.None;
    }
}