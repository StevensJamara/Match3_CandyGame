using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FindMatches : MonoBehaviour
{

    private Board board;
    public List<GameObject> currentMatches = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        board = FindObjectOfType<Board>();
    }

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCo());
    }

    private List<GameObject> IsAdjacentBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isAdjacentBomb)
        {
            currentDots = currentDots.Union(GetAdjacentPieces(dot1.column, dot1.row)).ToList();
        }

        if (dot2.isAdjacentBomb)
        {
            currentDots = currentDots.Union(GetAdjacentPieces(dot2.column, dot2.row)).ToList();
        }

        if (dot3.isAdjacentBomb)
        {
            currentDots = currentDots.Union(GetAdjacentPieces(dot3.column, dot3.row)).ToList();
        }
        return currentDots;
    }

    private List<GameObject> IsRowBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isRowBomb)
        {
            currentDots = currentDots.Union(GetRowPieces(dot1.row)).ToList();
        }

        if (dot2.isRowBomb)
        {
            currentDots = currentDots.Union(GetRowPieces(dot2.row)).ToList();
        }

        if (dot3.isRowBomb)
        {
            currentDots = currentDots.Union(GetRowPieces(dot3.row)).ToList();
        }
        return currentDots;
    }

    private List<GameObject> IsColumnBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isColumnBomb)
        {
            currentDots = currentDots.Union(GetColumnPieces(dot1.column)).ToList();
        }

        if (dot2.isColumnBomb)
        {
            currentDots = currentDots.Union(GetColumnPieces(dot2.column)).ToList();
        }

        if (dot3.isColumnBomb)
        {
            currentDots = currentDots.Union(GetColumnPieces(dot3.column)).ToList();
        }
        return currentDots;
    }

    private void AddToListAndMatch(GameObject dot)
    {
        if (!currentMatches.Contains(dot))
        {
            currentMatches.Add(dot);
        }
        dot.GetComponent<Dot>().isMatched = true;
    }

    private void GetNearbyPieces(GameObject dot1, GameObject dot2, GameObject dot3)
    {
        AddToListAndMatch(dot1);
        AddToListAndMatch(dot2);
        AddToListAndMatch(dot3);
    }

    private IEnumerator FindAllMatchesCo()
    {
        currentMatches.Clear();

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                GameObject currentDot = board.allDots[i, j];

                if (currentDot != null)
                {
                    Dot currentDotDot = currentDot.GetComponent<Dot>();
                    if (i > 0 && i < board.width - 1)
                    {
                        GameObject leftDot = board.allDots[i - 1, j];
                        GameObject rightDot = board.allDots[i + 1, j];

                        if (leftDot != null && rightDot != null)
                        {
                            Dot rightDotDot = rightDot.GetComponent<Dot>();
                            Dot leftDotDot = leftDot.GetComponent<Dot>();
                            if (leftDot.tag == currentDot.tag && rightDot.tag == currentDot.tag)
                            {
                                currentMatches = currentMatches.Union(IsRowBomb(leftDotDot, currentDotDot, rightDotDot)).ToList();
                                currentMatches = currentMatches.Union(IsColumnBomb(leftDotDot, currentDotDot, rightDotDot)).ToList();
                                currentMatches = currentMatches.Union(IsAdjacentBomb(leftDotDot, currentDotDot, rightDotDot)).ToList();

                                GetNearbyPieces(leftDot, currentDot, rightDot);
                            }
                        }
                    }

                    if (j > 0 && j < board.height - 1)
                    {
                        GameObject upDot = board.allDots[i, j + 1];
                        GameObject downDot = board.allDots[i, j - 1];

                        if (upDot != null && downDot != null)
                        {
                            Dot downDotDot = downDot.GetComponent<Dot>();
                            Dot upDotDot = upDot.GetComponent<Dot>();
                            if (upDot.tag == currentDot.tag && downDot.tag == currentDot.tag)
                            {
                                currentMatches = currentMatches.Union(IsColumnBomb(upDotDot, currentDotDot, downDotDot)).ToList();
                                currentMatches = currentMatches.Union(IsRowBomb(upDotDot, currentDotDot, downDotDot)).ToList();
                                currentMatches = currentMatches.Union(IsAdjacentBomb(upDotDot, currentDotDot, downDotDot)).ToList();

                                GetNearbyPieces(upDot, currentDot, downDot);
                            }
                        }
                    }
                }
            }
        }
        yield return null;
    }

    public void MatchPiecesOfColor(string color)
    {
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                //Check if that piece exists
                if (board.allDots[i, j] != null)
                {
                    //Check the tag on that dot
                    if (board.allDots[i, j].tag == color)
                    {
                        //Set that dot to be matched
                        board.allDots[i, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
    }

    List<GameObject> GetAdjacentPieces(int column, int row)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = column - 1; i <= column + 1; i++)
        {
            for (int j = row - 1; j <= row + 1; j++)
            {
                //Check if the piece is inside the board
                if (i >= 0 && i < board.width && j >= 0 && j < board.height)
                {
                    if (board.allDots[i, j] != null)
                    {
                        dots.Add(board.allDots[i, j]);
                        board.allDots[i, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
        return dots;
    }

    List<GameObject> GetColumnPieces(int column)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < board.height; i++)
        {
            if (board.allDots[column, i] != null)
            {
                Dot dot = board.allDots[column, i].GetComponent<Dot>();
                if (dot.isRowBomb)
                {
                    dots.Union(GetRowPieces(i)).ToList();
                }

                dots.Add(board.allDots[column, i]);
                dot.isMatched = true;
            }
        }
        return dots;
    }

    List<GameObject> GetRowPieces(int row)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < board.width; i++)
        {
            if (board.allDots[i, row] != null)
            {
                Dot dot = board.allDots[i, row].GetComponent<Dot>();
                if (dot.isColumnBomb)
                {
                    dots.Union(GetColumnPieces(i)).ToList();
                }
                dots.Add(board.allDots[i, row]);
                dot.isMatched = true;
            }
        }
        return dots;
    }

    public void CheckBombs()
    {
        // Kiểm tra xem có currentDot không
        if (board.currentDot == null) return;

        // Kiểm tra match hình chữ L hoặc T
        if (IsLOrTMatch())
        {
            CreateBombFromMatch(MakeAdjacentBomb);
            return;
        }

        // Kiểm tra match 4 hoặc 7
        if (currentMatches.Count == 4 || currentMatches.Count == 7)
        {
            if (IsVerticalMatch())
            {
                CreateBombFromMatch(MakeColumnBomb);
            }
            else
            {
                CreateBombFromMatch(MakeRowBomb);
            }
            return;
        }

        // Match 5 tạo color bomb
        if (currentMatches.Count >= 5)
        {
            CreateBombFromMatch(MakeColorBomb);
        }
    }

    private void CreateBombFromMatch(System.Action<Dot> createBomb)
    {
        // Tìm dot phù hợp để tạo bomb
        Dot dotToMakeBomb = null;

        // Ưu tiên currentDot nếu nó được match
        if (board.currentDot.isMatched)
        {
            dotToMakeBomb = board.currentDot;
        }
        // Nếu không, thử otherDot
        else if (board.currentDot.otherDot != null)
        {
            Dot otherDot = board.currentDot.otherDot.GetComponent<Dot>();
            if (otherDot.isMatched)
            {
                dotToMakeBomb = otherDot;
            }
        }
        // Nếu cả hai đều không match, lấy dot đầu tiên trong danh sách match
        else if (currentMatches.Count > 0)
        {
            dotToMakeBomb = currentMatches[0].GetComponent<Dot>();
        }

        // Tạo bomb nếu tìm được dot phù hợp
        if (dotToMakeBomb != null)
        {
            dotToMakeBomb.isMatched = false;
            createBomb(dotToMakeBomb);
        }
    }

    private void MakeRowBomb(Dot dot)
    {
        dot.MakeRowBomb();
    }

    private void MakeColumnBomb(Dot dot)
    {
        dot.MakeColumnBomb();
    }

    private void MakeAdjacentBomb(Dot dot)
    {
        dot.MakeAdjacentBomb();
    }

    private void MakeColorBomb(Dot dot)
    {
        dot.MakeColorBomb();
    }

    private bool IsVerticalMatch()
    {
        // Kiểm tra xem có phải match dọc không
        if (currentMatches.Count < 2) return false;

        // Lấy 2 dot đầu tiên để kiểm tra
        Dot firstDot = currentMatches[0].GetComponent<Dot>();
        Dot secondDot = currentMatches[1].GetComponent<Dot>();

        // Nếu cùng cột thì là match dọc
        return firstDot.column == secondDot.column;
    }

    private bool IsLOrTMatch()
    {
        if (currentMatches.Count < 5) return false;

        // Tạo dictionary để đếm số dot trên mỗi hàng và cột
        Dictionary<int, int> rowCounts = new Dictionary<int, int>();
        Dictionary<int, int> colCounts = new Dictionary<int, int>();

        foreach (GameObject match in currentMatches)
        {
            Dot dot = match.GetComponent<Dot>();

            // Đếm số dot trên mỗi hàng
            if (!rowCounts.ContainsKey(dot.row))
                rowCounts[dot.row] = 0;
            rowCounts[dot.row]++;

            // Đếm số dot trên mỗi cột
            if (!colCounts.ContainsKey(dot.column))
                colCounts[dot.column] = 0;
            colCounts[dot.column]++;
        }

        // Kiểm tra hình chữ L hoặc T
        bool hasThreeInRow = false;
        bool hasThreeInCol = false;

        foreach (var count in rowCounts.Values)
        {
            if (count >= 3) hasThreeInRow = true;
        }

        foreach (var count in colCounts.Values)
        {
            if (count >= 3) hasThreeInCol = true;
        }

        // Nếu có cả match 3 ngang và dọc thì là hình chữ L hoặc T
        return hasThreeInRow && hasThreeInCol;
    }

}