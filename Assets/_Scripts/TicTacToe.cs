using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToe : MonoBehaviour
{
    /// <summary>
    /// Variables
    /// </summary>
    public GameObject cellPrefab;
    public GameObject boardPanel;
    int boardSize;
    private string[,] board;
    private List<Cell> cellList = new List<Cell>();
    private List<Vector2Int> winningPositions;
    private int lengthToWin = 5;

    private string playerSymbol = "X";
    private string aiSymbol = "O";
    public Sprite playerSprite;
    public Sprite aiSprite;
    private readonly int[] columnDirections = { 0, 1, 1, -1 };
    private readonly int[] rowDirections = { 1, 0, 1, 1 };
    public bool isGameActive = true;
    public bool isGameOver = false;
    public bool isPlayerTurn = true;

    public GameObject resultPanel;
    public TMP_Text resultText;

    [Header("Visual Effects")]
    public Color winningCellColor = Color.green;
    public Color normalCellColor = Color.white;
    public float highlightDuration = 0.3f;

    public Button undoButton;
    public Button redoButton;

    // Stack lưu lịch sử để hỗ trợ Undo
    private Stack<(int row, int col, string symbol)> moveHistory = new();
    // Stack lưu lại các bước đã undo để hỗ trợ Redo
    private Stack<(int row, int col, string symbol)> redoHistory = new();
    // Lưu vị trí mới nhất vừa đánh (để tô màu)
    private Cell lastPlayedCell = null;

    public Image whoseTurn;

    private bool isCanUndo;

    StringBuilder winMessage = new StringBuilder();


    /// <summary>
    /// Functions
    /// </summary>
    private void Start()
    {
        CreateBoard(PlayerPrefs.GetInt("BoardSize", 20));
        undoButton.onClick.AddListener(Undo);
        redoButton.onClick.AddListener(Redo);
        isCanUndo = true;
    }

    // Tạo bàn cờ
    public void CreateBoard(int boardSizeToCreate)
    {
        // xóa bàn cờ cũ nếu có
        foreach (Transform child in boardPanel.transform)
        {
            Destroy(child.gameObject);
        }
        isGameActive = true;
        isPlayerTurn = false;
        board = null;
        cellList.Clear();
        winningPositions = new List<Vector2Int>();
        winMessage.Clear();
        whoseTurn.gameObject.SetActive(true);
        whoseTurn.sprite = isPlayerTurn ? playerSprite : aiSprite;

        resultPanel.SetActive(false);
        this.boardSize = boardSizeToCreate;
        board = new string[boardSize, boardSize];
        for (int row = 0; row < boardSize; row++)
        {
            for (int column = 0; column < boardSize; column++)
            {
                GameObject cell = Instantiate(cellPrefab, boardPanel.transform);
                Cell cellButton = cell.GetComponent<Cell>();
                cellButton.row = row;
                cellButton.column = column;
                cellButton.gameManager = this;

                int columnIndex = column;
                int rowIndex = row;
                cellButton.GetComponent<Button>().onClick.AddListener(() => HandlePlayerMove(rowIndex, columnIndex));
                cellList.Add(cellButton);
            }
        }
        boardPanel.GetComponent<GridLayoutGroup>().constraintCount = boardSize;
        int cellSize = 1000 / boardSize;
        boardPanel.GetComponent<GridLayoutGroup>().cellSize = new Vector2(cellSize - 5, cellSize - 5);
        HandleAIMove();

    }
    // Xử lý khi người chơi thực hiện nước đi tại vị trí (row, column).
    public void HandlePlayerMove(int row, int column)
    {
        if (!isGameActive || !isPlayerTurn || IsBoardFull(board) || board[row, column] != null)
        {
            Debug.Log("Invalid move");
            return;
        }

        board[row, column] = playerSymbol;
        AudioManager.instance.PlayClickSound();
        UpdateCellUI(row, column, playerSprite);
        moveHistory.Push((row, column, playerSymbol));
        redoHistory.Clear();  // xóa redo khi có bước mới

        if (CheckWin(playerSymbol))
        {
            resultPanel.SetActive(true);
            HighlightWinningCells();
            resultText.text = winMessage.Append("You Win!!!").ToString();
            AudioManager.instance.PlayWinSound();
            isGameActive = false;
        }
        else if (IsBoardFull(board))
        {
            DrawGame();
        }
        else
        {
            isPlayerTurn = !isPlayerTurn;
            whoseTurn.sprite = aiSprite;
            Invoke(nameof(HandleAIMove), 2f);
        }
    }
    // Kiểm tra xem symbolToWin có thắng trên bàn cờ hay không.
    bool CheckWin(string symbolToWin, string[,] b = null)
    {
        b ??= board;
        winningPositions.Clear(); // Xóa danh sách trước khi kiểm tra

        for (int row = 0; row < boardSize; row++)
        {
            for (int column = 0; column < boardSize; column++)
            {
                if (b[row, column] != symbolToWin)
                {
                    continue;
                }

                for (int direction = 0; direction < 4; direction++)
                {
                    int count = 1;
                    winningPositions.Add(new Vector2Int(row, column)); // Thêm vị trí đầu tiên

                    for (int step = 1; step < lengthToWin; step++)
                    {
                        int newRow = row + step * rowDirections[direction];
                        int newColumn = column + step * columnDirections[direction];

                        if (!IsInBounds(newRow, newColumn) || b[newRow, newColumn] != symbolToWin)
                            break;
                        count++;
                        winningPositions.Add(new Vector2Int(newRow, newColumn)); // Thêm vị trí thắng
                    }

                    if (count >= lengthToWin)
                    {
                        return true;
                    }
                    winningPositions.Clear(); // Xóa nếu không thắng
                }
            }
        }
        return false;
    }
    // Kiểm tra vị trí (row, column) có nằm trong phạm vi bàn cờ không.
    bool IsInBounds(int row, int column)
    {
        return row >= 0 && row < boardSize && column >= 0 && column < boardSize;
    }
    // Cập nhật giao diện ô tại vị trí (row, column) với ký hiệu symbol.
    void UpdateCellUI(int row, int column, Sprite sprite)
    {
        var cell = cellList.Find(c => c.row == row && c.column == column);
        if (cell != null)
        {
            cell.SetCellState(sprite);
            StartCoroutine(HighlightLastMove(cell));
        }
    }
    // Kiểm tra xem bàn cờ đã đầy chưa (không còn ô trống).
    bool IsBoardFull(string[,] board)
    {
        for (int row = 0; row < boardSize; row++)
        {
            for (int column = 0; column < boardSize; column++)
            {
                if (string.IsNullOrEmpty(board[row, column]))
                {
                    return false;
                }
            }
        }
        Debug.Log("Bàn cờ đã đầy");
        return true;
    }
    void DrawGame()
    {
        resultPanel.SetActive(true);
        resultPanel.GetComponentInChildren<TMP_Text>().text = "Draw!!!";
    }
    int CountChessOnBoard(string[,] b = null)
    {
        int count = 0;
        foreach (var cell in b)
        {
            if (!string.IsNullOrEmpty(cell))
            {
                count++;
            }
        }
        return count;
    }
    IEnumerator HighlightLastMove(Cell cell)
    {
        lastPlayedCell = cell;
        var image = cell.transform.GetChild(0).GetComponent<Image>();
        image.gameObject.SetActive(true);
        Color originalColor = image.color;
        image.color = Color.yellow;
        yield return new WaitForSeconds(1f);  // Hiệu ứng trong 1 giây
        if (/*lastPlayedCell == cell &&*/ !winningPositions.Contains(new Vector2Int(cell.row, cell.column)))
            image.gameObject.SetActive(false);
    }
    public void Undo()
    {
        if(!isCanUndo) return;

        for (int i = 0; i < 2; i++)
        {
            if (moveHistory.Count > 0 && isGameActive)
            {
                var (row, col, symbol) = moveHistory.Pop();
                board[row, col] = null;
                var cell = cellList.Find(c => c.row == row && c.column == col);
                if (cell != null)
                {
                    cell.SetCellState(null);
                    cell.GetComponent<Button>().interactable = true;
                    cell.transform.GetChild(0).gameObject.SetActive(false);
                    cell.transform.GetChild(1).gameObject.SetActive(false);
                }
                redoHistory.Push((row, col, symbol));
                isPlayerTurn = symbol == playerSymbol;
                whoseTurn.sprite = isPlayerTurn ? playerSprite : aiSprite;
            }
        }
    }
    public void Redo()
    {
        for(int i = 0; i < 2;i++)
        {
            if (redoHistory.Count > 0 && isGameActive)
            {
                var (row, col, symbol) = redoHistory.Pop();
                board[row, col] = symbol;
                var sprite = symbol == playerSymbol ? playerSprite : aiSprite;
                UpdateCellUI(row, col, sprite);
                moveHistory.Push((row, col, symbol));
                isPlayerTurn = symbol != playerSymbol;
                whoseTurn.sprite = isPlayerTurn ? playerSprite : aiSprite;
            }
        }
    }
    void HighlightWinningCells()
    {
        foreach (var pos in winningPositions)
        {
            var cell = cellList.Find(c => c.row == pos.x && c.column == pos.y);
            if (cell != null)
            {
                cell.transform.GetChild(0).gameObject.SetActive(true);
                cell.transform.GetChild(0).GetComponent<Image>().color = winningCellColor;
            }
        }
    }
    
    /// <summary>
    /// AI Behaviour
    /// </summary>
    // Xử lý lượt đi của AI.
    void HandleAIMove()
    {
        int stoneCount = CountChessOnBoard(board);
        // tăng độ sâu tìm kiếm
        int depth = stoneCount < 10 ? 4 : 3;

        // Tìm nước đi ngay lập tức để thắng hoặc chặn đối phương thắng.
        Vector2Int immediateMove = FindImediateMove();

        Vector2Int bestMove = Vector2Int.zero;
        if (immediateMove != Vector2Int.zero)
        {
            bestMove = immediateMove;
        }
        else
        {
            // tìm nước đi tốt nhất cho AI
            var (move, _) = Minimax(board, depth, true, int.MinValue, int.MaxValue);
            bestMove = move;
        }

        board[bestMove.x, bestMove.y] = aiSymbol;
        AudioManager.instance.PlayClickSound();
        UpdateCellUI(bestMove.x, bestMove.y, aiSprite);
        moveHistory.Push((bestMove.x, bestMove.y, aiSymbol));
        redoHistory.Clear();

        // Kiểm tra xem AI có thắng không
        if (CheckWin(aiSymbol))
        {
            resultPanel.SetActive(true);
            HighlightWinningCells();
            resultText.text = winMessage.Append("AI Win!!!").ToString();
            AudioManager.instance.PlayWinSound();
            isCanUndo = false;
            isGameActive = false;
        }
        else if (IsBoardFull(board))
        {
            DrawGame();
        }
        else
        {
            isPlayerTurn = !isPlayerTurn;
            whoseTurn.sprite = playerSprite;
        }

    }
    // Tìm nước đi ngay lập tức để thắng hoặc chặn đối phương thắng.
    Vector2Int FindImediateMove()
    {
        List<Vector2Int> dangerMoves = new List<Vector2Int>();

        // === Kiểm tra AI có thể thắng không ===
        for (int row = 0; row < boardSize; row++)
        {
            for (int column = 0; column < boardSize; column++)
            {
                if (board[row, column] != null)
                    continue;

                board[row, column] = aiSymbol;
                if (CheckWin(aiSymbol))
                {
                    board[row, column] = null;
                    return new Vector2Int(row, column); // AI thắng ngay lập tức
                }
                board[row, column] = null;
            }
        }

        // === Kiểm tra chặn người chơi nếu họ có thể thắng ===
        for (int row = 0; row < boardSize; row++)
        {
            for (int column = 0; column < boardSize; column++)
            {
                if (board[row, column] != null)
                    continue;

                board[row, column] = playerSymbol;
                if (CheckWin(playerSymbol))
                {
                    board[row, column] = null;
                    return new Vector2Int(row, column); // Chặn ngay lập tức
                }

                // Kiểm tra các vị trí nguy hiểm tiềm năng
                else if (CheckPotentialThreat(row, column, playerSymbol))
                {
                    dangerMoves.Add(new Vector2Int(row, column));
                }

                board[row, column] = null;
            }
        }

        // === Chọn vị trí nguy hiểm nhất để chặn ===
        if (dangerMoves.Count > 0)
        {
            // Chọn vị trí nguy hiểm nhất
            return dangerMoves.OrderByDescending(move => EvaluateMove(board, move.x, move.y)).First();
        }

        // === Kiểm tra AI có thể tạo thành 4 quân ===
        for (int row = 0; row < boardSize; row++)
        {
            for (int column = 0; column < boardSize; column++)
            {
                if (board[row, column] != null)
                    continue;

                if (IsThreeInARow(row, column, aiSymbol) == 3)
                {
                    return new Vector2Int(row, column); // Đánh để tạo thành 4 quân
                }
            }
        }

        // === Kiểm tra nếu người chơi có 2 quân liên tiếp ===
        for (int row = 0; row < boardSize; row++)
        {
            for (int column = 0; column < boardSize; column++)
            {
                if (board[row, column] != null)
                    continue;

                if (IsThreeInARow(row, column, playerSymbol) == 2)
                {
                    return new Vector2Int(row, column); // Chặn người chơi tạo thành 3 quân
                }
            }
        }

        // === Nếu không tìm thấy nước đi nào, trả về vị trí mặc định ===
        return Vector2Int.zero;
    }
    // Đánh giá điểm số của bàn cờ hiện tại cho AI và người chơi.
    int EvaluateBoard(string[,] b)
    {
        int score = 0;

        int[] aiCounts = { 0, 10, 50, 100, 500 };
        int[] playerCounts = { 0, -20, -75, -150, -750 };

        for (int row = 0; row < boardSize; row++)
        {
            for (int column = 0; column < boardSize; column++)
            {
                for (int direction = 0; direction < rowDirections.Length; direction++)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        if (row + rowDirections[d] * (lengthToWin - 1) >= boardSize ||
                           row + rowDirections[d] * (lengthToWin - 1) < 0 ||
                           column + columnDirections[d] * (lengthToWin - 1) >= boardSize ||
                           column + columnDirections[d] * (lengthToWin - 1) < 0)
                        {
                            continue;
                        }

                        int aiCount = 0, playerCount = 0, emtyCount = 0;

                        for (int step = 0; step < lengthToWin; step++)
                        {
                            int newRow = row + step * rowDirections[d];
                            int newColumn = column + step * columnDirections[d];

                            if (IsInBounds(newRow, newColumn))
                            {
                                if (b[newRow, newColumn] == aiSymbol)
                                    aiCount++;
                                else if (b[newRow, newColumn] == playerSymbol)
                                    playerCount++;
                                else
                                    emtyCount++;
                            }
                        }
                        // Chỉ tính điểm nếu không bị chặn
                        if (playerCount == 0 && aiCount > 0)
                        {
                            if (aiCount < lengthToWin)
                            {
                                score += aiCounts[aiCount];
                            }
                        }
                        else if (aiCount == 0 && playerCount > 0)
                        {
                            if (playerCount < lengthToWin)
                            {
                                score += playerCounts[playerCount];
                            }
                        }
                    }
                }
            }
        }

        // Thêm yếu tố vị trí - ưu tiên các ô giữa
        for (int row = 0; row < boardSize; row++)
        {
            for (int column = 0; column < boardSize; column++)
            {
                int centerRow = boardSize / 2;
                int centerColumn = boardSize / 2;

                float distanceToCenter = Mathf.Sqrt(Mathf.Pow(row - centerRow, 2) + Mathf.Pow(column - centerColumn, 2));

                // Tính điểm dựa trên khoảng cách đến tâm
                score += Mathf.Max(5 - (int)distanceToCenter, 0);
            }
        }

        return score;
    }
    // Tính sức mạnh của một nước đi tại vị trí (row, column) với ký hiệu symbol.
    int CalculateStrength(string[,] b, int row, int column, string symbol)
    {
        int strength = 0;
        for (int direction = 0; direction < rowDirections.Length; direction++)
        {
            for (int step = 0; step < lengthToWin; step++)
            {
                int count = 1;

                int emptyBefore = 0;
                int emptyAfter = 0;

                for (int i = 1; i < lengthToWin; i++)
                {
                    int newRow = row + i * rowDirections[direction];
                    int newColumn = column + i * columnDirections[direction];
                    if (!IsInBounds(newRow, newColumn))
                        break;
                    if (b[newRow, newColumn] == symbol)
                    {
                        count++;
                    }
                    else if (b[newRow, newColumn] == null)
                    {
                        emptyAfter++;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
                if (count + emptyBefore + emptyAfter >= lengthToWin)
                {
                    if (count == 4) strength += 1000;
                    else if (count == 3) strength += 100;
                    else if (count == 2) strength += 10;
                    else if (count == 1) strength += 1;

                    strength += (emptyBefore + emptyAfter) * 2;
                }

            }
        }
        return strength;
    }
    // Đánh giá giá trị của một nước đi tại vị trí (row, column) cho AI.
    int EvaluateMove(string[,] b, int row, int column)
    {
        int value = 0;
        b[row, column] = aiSymbol;
        value += CalculateStrength(b, row, column, aiSymbol);
        b[row, column] = null;

        b[row, column] = playerSymbol;
        value -= CalculateStrength(b, row, column, playerSymbol);
        b[row, column] = null;

        int centerRow = boardSize / 2;
        int centerColumn = boardSize / 2;

        float distanceToCenter = Mathf.Sqrt(Mathf.Pow(row - centerRow, 2) + Mathf.Pow(column - centerColumn, 2));

        value += Mathf.Max(5 - (int)distanceToCenter, 0);

        return value;
    }
    // Lấy danh sách các nước đi tiềm năng thông minh cho AI dựa trên các ô trống gần quân cờ đã có.
    List<Vector2Int> GetSmartCandidateMove(string[,] b)
    {
        List<Vector2Int> availableMoves = new List<Vector2Int>();
        HashSet<Vector2Int> consideredCells = new HashSet<Vector2Int>();
        int searchRadius = 2;

        // tìm kiếm các ô trống xung quanh các ô đã có quân cờ
        for (int row = 0; row < boardSize; row++)
        {
            for (int column = 0; column < boardSize; column++)
            {
                if (b[row, column] != null)
                {
                    for (int r = -searchRadius; r <= searchRadius; r++)
                    {
                        for (int c = -searchRadius; c <= searchRadius; c++)
                        {
                            int newRow = row + r;
                            int newColumn = column + c;
                            if (IsInBounds(newRow, newColumn) && b[newRow, newColumn] == null && !consideredCells.Contains(new Vector2Int(newRow, newColumn)))
                            {
                                availableMoves.Add(new Vector2Int(newRow, newColumn));
                                consideredCells.Add(new Vector2Int(newRow, newColumn));

                                int value = EvaluateMove(b, newRow, newColumn);
                            }
                        }
                    }
                }
            }
        }

        // Nếu không tìm thấy nước đi nào, thì tìm nước đi ngẫu nhiên tại trung tâm bàn cờ
        if (availableMoves.Count == 0)
        {
            int centerRow = boardSize / 2;
            int centerColumn = boardSize / 2;
            if (b[centerRow, centerColumn] == null)
            {
                availableMoves.Add(new Vector2Int(centerRow, centerColumn));
            }
            else
            {
                for (int row = 0; row < boardSize; row++)
                {
                    for (int column = 0; column < boardSize; column++)
                    {
                        if (b[row, column] == null)
                        {
                            availableMoves.Add(new Vector2Int(row, column));
                            break;
                        }
                    }
                    if (availableMoves.Count > 0)
                    {
                        break;
                    }
                }
            }
        }
        // Sắp xếp danh sách các nước đi theo giá trị đánh giá giảm dần
        availableMoves = availableMoves.OrderByDescending(move => EvaluateMove(b, move.x, move.y)).ToList();

        return availableMoves;
    }
    //Hàm Minimax 
    (Vector2Int move, int score) Minimax(string[,] b, int depth, bool isMaximizing, int alpha, int beta)
    {
        if (CheckWin(aiSymbol, b))
        {
            return (Vector2Int.zero, 1000 + depth);
        }
        if (CheckWin(playerSymbol, b))
        {
            return (Vector2Int.zero, -1000 - depth);
        }
        if (depth == 0 || IsBoardFull(b))
        {
            return (Vector2Int.zero, EvaluateBoard(b));
        }

        // Tìm các nước đi tiềm năng thông minh cho AI
        List<Vector2Int> availableMoves = GetSmartCandidateMove(b);
        Vector2Int bestMove = availableMoves.Count > 0 ? availableMoves[0] : Vector2Int.zero;
        int bestValue = isMaximizing ? int.MinValue : int.MaxValue;

        foreach (Vector2Int move in availableMoves)
        {
            if (IsInBounds(move.x, move.y))
            {
                if (IsInBounds(move.x, move.y))
                {
                    b[move.x, move.y] = isMaximizing ? aiSymbol : playerSymbol;
                }
                var value = Minimax(b, depth - 1, !isMaximizing, alpha, beta).score;
                b[move.x, move.y] = null;

                if (isMaximizing && value > bestValue)
                {
                    bestValue = value;
                    bestMove = move;
                    alpha = Mathf.Max(alpha, value);
                }
                else
                {
                    if (value < bestValue)
                    {
                        bestValue = value;
                        bestMove = move;
                    }
                    beta = Mathf.Min(beta, value);
                }
                if (beta <= alpha) // cắt tỉa alpha-beta
                {
                    break;
                }
            }
        }
        return (bestMove, bestValue);
    }
    // Kiểm tra nếu người chơi có 3 quân liên tiếp 
    bool CheckPotentialThreat(int row, int column, string symbol)
    {
        for (int direction = 0; direction < 4; direction++)
        {
            int count = 0;

            for (int step = -1; step < lengthToWin; step++)
            {
                int newRow = row + step * rowDirections[direction];
                int newColumn = column + step * columnDirections[direction];

                if (!IsInBounds(newRow, newColumn))
                    continue;

                if (board[newRow, newColumn] == symbol)
                {
                    count++;
                }
                else if (board[newRow, newColumn] == null && count == 4)
                {
                    // Nếu tìm thấy 4 quân liên tiếp và 1 ô trống, chặn ngay
                    return true;
                }
                else
                {
                    count = 0;
                }
            }
        }
        return false;
    }

    // Hàm kiểm tra nếu có 3 quân liền nhau
    int IsThreeInARow(int row, int column, string symbol)
    {
        for (int direction = 0; direction < rowDirections.Length; direction++)
        {
            int count = 0;

            for (int step = -2; step <= 2; step++) // Kiểm tra 2 ô trước và 2 ô sau
            {
                if (step == 0) continue; // Bỏ qua ô hiện tại

                int newRow = row + step * rowDirections[direction];
                int newColumn = column + step * columnDirections[direction];

                if (IsInBounds(newRow, newColumn) && board[newRow, newColumn] == symbol)
                {
                    count++;
                }
            }

            if (count >= 2) // Nếu có 2 quân liền nhau
                return 3; // Có thể tạo thành 3 quân
            else if (count > 1)
                return 2; // Nếu có 2 quân liền nhau thì có thể tạo thành 3 quân
        }
        return 1;
    }
}
