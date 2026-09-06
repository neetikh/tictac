namespace TicTacToe.Domain;

public enum Player
{
    X,
    O
}

public enum GameMode
{
    TwoPlayer,
    Computer
}

public enum GameStatus
{
    InProgress,
    Won,
    Draw
}

public sealed record CellPosition(int Row, int Column);

public sealed record MoveRecord(int MoveNumber, Player Player, int Row, int Column)
{
    public string Position => $"Row {Row + 1}, Column {Column + 1}";
}

public sealed record ScoreboardSnapshot(int XWins, int OWins, int Draws);

public sealed record GameSnapshot(
    Guid Id,
    Player?[][] Board,
    Player CurrentPlayer,
    GameMode Mode,
    GameStatus Status,
    Player? Winner,
    IReadOnlyList<CellPosition> WinningCells,
    IReadOnlyList<MoveRecord> MoveHistory,
    bool CanUndo,
    ScoreboardSnapshot Scoreboard);

public sealed class GameException : Exception
{
    public GameException(string message) : base(message)
    {
    }
}

public sealed class GameNotFoundException : Exception
{
    public GameNotFoundException(Guid id) : base($"Game '{id}' was not found.")
    {
    }
}
