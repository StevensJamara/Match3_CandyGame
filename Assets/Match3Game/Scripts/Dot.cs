using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    [Header("Board Variables")]
    public int column;
    public int row;
    public int previousColumn;
    public int previousRow;
    public int targetX;
    public int targetY;
    public bool isMatched = false;

    private HintTiles hintManager;
    private FindMatches findMatches;
    private Board board;
    public GameObject otherDot;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;

    [Header("Swipe Stuff")]
    public float swipeAngle = 0;
    public float swipeResist = 1f;

    [Header("Powerup Stuff")]
    public bool isColorBomb;
    public bool isColumnBomb;
    public bool isRowBomb;
    public bool isAdjacentBomb;
    public GameObject adjacentMarker;
    public GameObject rowArrow;
    public GameObject columnArrow;
    public GameObject colorBomb;

    [Header("TouchEvent")]
    private Vector2 touchPosition;
    private bool isSelected = false;
    private Color originalColor;


    // Use this for initialization
    void Start()
    {
        isColumnBomb = false;
        isRowBomb = false;
        isColorBomb = false;
        isAdjacentBomb = false;

        hintManager = FindObjectOfType<HintTiles>();
        board = FindObjectOfType<Board>();
        findMatches = FindObjectOfType<FindMatches>();

        // Lưu màu gốc khi khởi tạo
        originalColor = GetComponent<SpriteRenderer>().color;
    }


    //This is for testing and Debug only.
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isAdjacentBomb = true;
            GameObject marker = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
            marker.transform.parent = this.transform;
        }
    }


    // Update is called once per frame
    void Update()
    {
        /*
        if(isMatched){
            
            SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
            Color currentColor = mySprite.color;
            mySprite.color = new Color(currentColor.r, currentColor.g, currentColor.b, .5f);
        }
        */
        targetX = column;
        targetY = row;
        if (Mathf.Abs(targetX - transform.position.x) > .1)
        {
            //Move Towards the target
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();


        }
        else
        {
            //Directly set the position
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;

        }
        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {
            //Move Towards the target
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();

        }
        else
        {
            //Directly set the position
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;

        }
    }

    public IEnumerator CheckMoveCo()
    {
        if (isColorBomb)
        {
            //This piece is a color bomb, and the other piece is the color to destroy
            findMatches.MatchPiecesOfColor(otherDot.tag);
            isMatched = true;
        }
        else if (otherDot.GetComponent<Dot>().isColorBomb)
        {
            //The other piece is a color bomb, and this piece has the color to destroy
            findMatches.MatchPiecesOfColor(this.gameObject.tag);
            otherDot.GetComponent<Dot>().isMatched = true;
        }
        yield return new WaitForSeconds(.5f);
        if (otherDot != null)
        {
            if (!isMatched && !otherDot.GetComponent<Dot>().isMatched)
            {
                otherDot.GetComponent<Dot>().row = row;
                otherDot.GetComponent<Dot>().column = column;
                row = previousRow;
                column = previousColumn;
                yield return new WaitForSeconds(.5f);
                board.currentDot = null;
                board.currentState = GameState.move;
            }
            else
            {
                board.DestroyMatches();
            }

            // Đảm bảo cả hai dot đều trở về màu gốc
            DeselectDot();
            otherDot.GetComponent<Dot>().DeselectDot();
        }
    }

    private void OnMouseDown()
    {
        //Destroy the hint
        if (hintManager != null)
        {
            hintManager.DestroyHint();
        }

        if (board.currentState == GameState.move)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void OnMouseUp()
    {
        if (board.currentState == GameState.move)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }

    void CalculateAngle()
    {
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            board.currentState = GameState.wait;
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            MovePieces();
            board.currentDot = this;
        }
        else
        {
            board.currentState = GameState.move;
        }
    }

    void MovePiecesActual(Vector2 direction)
    {
        otherDot = board.allDots[column + (int)direction.x, row + (int)direction.y];
        previousRow = row;
        previousColumn = column;
        if (otherDot != null)
        {
            otherDot.GetComponent<Dot>().column += -1 * (int)direction.x;
            otherDot.GetComponent<Dot>().row += -1 * (int)direction.y;
            column += (int)direction.x;
            row += (int)direction.y;
            StartCoroutine(CheckMoveCo());
        }
        else
        {
            board.currentState = GameState.move;
        }
    }

    void MovePieces()
    {
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1)
        {
            //Right Swipe
            /*
            otherDot = board.allDots[column + 1, row];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dot>().column -=1;
            column += 1;
            StartCoroutine(CheckMoveCo());
            */
            MovePiecesActual(Vector2.right);
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
        {
            //Up Swipe
            /*
            otherDot = board.allDots[column, row + 1];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dot>().row -=1;
            row += 1;
            StartCoroutine(CheckMoveCo());
            */
            MovePiecesActual(Vector2.up);
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            //Left Swipe
            /*
            otherDot = board.allDots[column - 1, row];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dot>().column +=1;
            column -= 1;
            StartCoroutine(CheckMoveCo());
            */
            MovePiecesActual(Vector2.left);
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            //Down Swipe
            /*
            otherDot = board.allDots[column, row - 1];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dot>().row +=1;
            row -= 1;
            StartCoroutine(CheckMoveCo());
            */
            MovePiecesActual(Vector2.down);
        }
        else
        {

            board.currentState = GameState.move;
        }

    }

    void FindMatches()
    {
        if (column > 0 && column < board.width - 1)
        {
            GameObject leftDot1 = board.allDots[column - 1, row];
            GameObject rightDot1 = board.allDots[column + 1, row];
            if (leftDot1 != null && rightDot1 != null)
            {
                if (leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag)
                {
                    leftDot1.GetComponent<Dot>().isMatched = true;
                    rightDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
        if (row > 0 && row < board.height - 1)
        {
            GameObject upDot1 = board.allDots[column, row + 1];
            GameObject downDot1 = board.allDots[column, row - 1];
            if (upDot1 != null && downDot1 != null)
            {
                if (upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag)
                {
                    upDot1.GetComponent<Dot>().isMatched = true;
                    downDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }

    }

    public void MakeRowBomb()
    {
        isRowBomb = true;
        GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    public void MakeColumnBomb()
    {
        isColumnBomb = true;
        GameObject arrow = Instantiate(columnArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    public void MakeColorBomb()
    {
        isColorBomb = true;
        GameObject color = Instantiate(colorBomb, transform.position, Quaternion.identity);
        color.transform.parent = this.transform;
        this.gameObject.tag = "Color";
    }

    public void MakeAdjacentBomb()
    {
        isAdjacentBomb = true;
        GameObject marker = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
        marker.transform.parent = this.transform;
    }

    public void checkCurrentTouch()
    {
        if (Input.touchCount > 0 && board.currentState == GameState.move)
        {
            Touch touch = Input.GetTouch(0);
            touchPosition = Camera.main.ScreenToWorldPoint(touch.position);

            if (touch.phase == TouchPhase.Began)
            {
                // Kiểm tra nếu chạm vào dot này
                if (IsPointOverDot(touchPosition))
                {
                    // Nếu chưa có dot nào được chọn
                    if (!isSelected)
                    {
                        SelectDot();
                    }
                    // Nếu đã có dot được chọn và chạm vào dot khác
                    else
                    {
                        // Kiểm tra xem dot mới chạm có kề với dot đã chọn không
                        if (IsNextTo())
                        {
                            SwapDots();
                        }
                        // Nếu không kề thì bỏ chọn dot cũ
                        else
                        {
                            DeselectDot();
                        }
                    }
                }
            }
        }
    }

    public bool IsPointOverDot(Vector2 point)
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        return sprite.bounds.Contains(point);
    }

    public void SelectDot()
    {
        if (isSelected) return;  // Nếu đã được chọn thì không làm gì

        isSelected = true;
        // Lưu màu gốc và làm sáng dot
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.color = new Color(originalColor.r + 0.2f, originalColor.g + 0.2f, originalColor.b + 0.2f, originalColor.a);
        board.currentDot = this;
    }

    public void DeselectDot()
    {
        if (!isSelected) return;  // Nếu chưa được chọn thì không làm gì

        isSelected = false;
        // Trả về màu gốc của sprite
        GetComponent<SpriteRenderer>().color = originalColor;
        board.currentDot = null;
    }

    public bool IsNextTo()
    {
        Dot otherDot = board.currentDot;

        // Kiểm tra xem có phải ô kề không
        if (Mathf.Abs(column - otherDot.column) + Mathf.Abs(row - otherDot.row) == 1)
        {
            return true;
        }
        return false;
    }

    public void SwapDots()
    {
        // Chuyển game state sang wait khi bắt đầu swap
        board.currentState = GameState.wait;

        Dot otherDot = board.currentDot;

        // Lưu vị trí tạm thời
        int tempColumn = column;
        int tempRow = row;

        // Đổi vị trí
        column = otherDot.column;
        row = otherDot.row;
        otherDot.column = tempColumn;
        otherDot.row = tempRow;

        // Cập nhật trong mảng board
        board.allDots[column, row] = this.gameObject;
        board.allDots[otherDot.column, otherDot.row] = otherDot.gameObject;

        // Set currentDot cho Board để kiểm tra tạo bomb
        board.currentDot = this;
        otherDot.otherDot = this.gameObject;
        this.otherDot = otherDot.gameObject;

        // Kiểm tra match sau khi swap
        StartCoroutine(CheckMatchesAfterSwap(otherDot));
    }

    private IEnumerator CheckMatchesAfterSwap(Dot otherDot)
    {
        yield return new WaitForSeconds(0.5f);

        board.findMatches.FindAllMatches();

        // Nếu không có match nào
        if (board.findMatches.currentMatches.Count == 0)
        {
            // Hoán đổi lại vị trí
            int tempColumn = column;
            int tempRow = row;

            column = otherDot.column;
            row = otherDot.row;
            otherDot.column = tempColumn;
            otherDot.row = tempRow;

            // Cập nhật lại mảng board
            board.allDots[column, row] = this.gameObject;
            board.allDots[otherDot.column, otherDot.row] = otherDot.gameObject;

            // Chuyển game state về move sau khi hoàn tất
            board.currentState = GameState.move;
        }
        else
        {
            // Kiểm tra và tạo bomb trước khi destroy matches
            board.findMatches.CheckBombs();
            board.DestroyMatches();
        }

        // Reset các biến sau khi hoàn thành
        this.otherDot = null;
        otherDot.otherDot = null;
        board.currentDot = null;

        // Đảm bảo cả hai dot đều trở về màu gốc
        DeselectDot();
        otherDot.DeselectDot();
    }
}