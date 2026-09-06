namespace TicTacToe.Domain;

public sealed class Game
{
    private readonly Player?[] _board = new Player?[9];
    private readonly List<MoveRecord> _history = [];

    public Guid Id { get; } = Guid.NewGuid();
    public GameMode Mode { get; }
    public Player CurrentPlayer { get; private set; } = Player.X;
    public GameStatus Status { get; private set; } = GameStatus.InProgress;
    public Player? Winner { get; private set; }
    public IReadOnlyList<CellPosition> WinningCells { get; private set; } = [];
    public IReadOnlyList<MoveRecord> MoveHistory => _history;
    public bool CanUndo => _history.Count > 0;
    public bool OutcomeCounted { get; private set; }

    public Game(GameMode mode)
    {
        Mode = mode;
    }

    public Player?[][] BoardAsGrid()
    {
        var grid = new Player?[3][];
        for (var row = 0; row < 3; row++)
        {
            grid[row] = new Player?[3];
            for (var col = 0; col < 3; col++)
            {
                grid[row][col] = _board[row * 3 + col];
            }
        }

        return grid;
    }

    public IReadOnlyList<Player?> BoardSnapshot() => _board.ToArray();

    public void Play(Player player, int row, int col, IComputerPlayer computer)
    {
        if (Mode == GameMode.Computer && player == Player.O)
        {
            throw new GameException("O is controlled by the computer.");
        }

        ApplyMove(player, row, col);

        if (Mode == GameMode.Computer && Status == GameStatus.InProgress)
        {
            var index = computer.ChooseMove(_board);
            ApplyMove(Player.O, index / 3, index % 3);
        }
    }

    public void Undo()
    {
        if (_history.Count == 0)
        {
            throw new GameException("There are no moves to undo.");
        }

        var movesToRemove = Mode == GameMode.Computer
            ? Math.Min(2, _history.Count)
            : 1;

        for (var i = 0; i < movesToRemove; i++)
        {
            RevertLastMove();
        }

        RecalculateStatus();
        CurrentPlayer = Mode == GameMode.Computer || _history.Count == 0
            ? Player.X
            : Opponent(_history[^1].Player);
    }

    public void Reset()
    {
        Array.Clear(_board);
        _history.Clear();
        CurrentPlayer = Player.X;
        Status = GameStatus.InProgress;
        Winner = null;
        WinningCells = [];
        OutcomeCounted = false;
    }

    public void MarkOutcomeCounted() => OutcomeCounted = true;

    public void ClearOutcomeCounted() => OutcomeCounted = false;

    private void ApplyMove(Player player, int row, int col)
    {
        if (row is < 0 or > 2 || col is < 0 or > 2)
        {
            throw new GameException("Move is outside the board.");
        }

        if (Status != GameStatus.InProgress)
        {
            throw new GameException("The game is already completed.");
        }

        if (player != CurrentPlayer)
        {
            throw new GameException($"It is {CurrentPlayer}'s turn.");
        }

        var index = row * 3 + col;
        if (_board[index] is not null)
        {
            throw new GameException("That cell is already occupied.");
        }

        _board[index] = player;
        _history.Add(new MoveRecord(_history.Count + 1, player, row, col));
        RecalculateStatus();

        if (Status == GameStatus.InProgress)
        {
            CurrentPlayer = Opponent(player);
        }
    }

    private void RevertLastMove()
    {
        var last = _history[^1];
        _history.RemoveAt(_history.Count - 1);
        _board[last.Row * 3 + last.Column] = null;
    }

    private void RecalculateStatus()
    {
        var (winner, cells) = WinDetector.Find(_board);
        if (winner is not null)
        {
            Status = GameStatus.Won;
            Winner = winner;
            WinningCells = cells;
            return;
        }

        if (_board.All(cell => cell is not null))
        {
            Status = GameStatus.Draw;
            Winner = null;
            WinningCells = [];
            return;
        }

        Status = GameStatus.InProgress;
        Winner = null;
        WinningCells = [];
    }

    private static Player Opponent(Player player) => player == Player.X ? Player.O : Player.X;
}
