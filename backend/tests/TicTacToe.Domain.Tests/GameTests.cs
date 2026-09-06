using TicTacToe.Domain;

namespace TicTacToe.Domain.Tests;

public class GameTests
{
    private static readonly IComputerPlayer Computer = new ComputerPlayer();

    [Fact]
    public void Play_ValidMove_PlacesMarkAndSwitchesTurn()
    {
        var game = new Game(GameMode.TwoPlayer);

        game.Play(Player.X, 0, 0, Computer);

        Assert.Equal(Player.X, game.BoardAsGrid()[0][0]);
        Assert.Equal(Player.O, game.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Single(game.MoveHistory);
        Assert.Equal(1, game.MoveHistory[0].MoveNumber);
        Assert.Equal(Player.X, game.MoveHistory[0].Player);
        Assert.Equal(0, game.MoveHistory[0].Row);
        Assert.Equal(0, game.MoveHistory[0].Column);
        Assert.Equal("Row 1, Column 1", game.MoveHistory[0].Position);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(3, 0)]
    [InlineData(0, 3)]
    public void Play_OutsideBoard_Throws(int row, int column)
    {
        var game = new Game(GameMode.TwoPlayer);

        var ex = Assert.Throws<GameException>(() => game.Play(Player.X, row, column, Computer));

        Assert.Equal("Move is outside the board.", ex.Message);
        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Empty(game.MoveHistory);
    }

    [Fact]
    public void Play_OccupiedCell_ThrowsAndKeepsTurn()
    {
        var game = new Game(GameMode.TwoPlayer);
        game.Play(Player.X, 1, 1, Computer);

        var ex = Assert.Throws<GameException>(() => game.Play(Player.O, 1, 1, Computer));

        Assert.Equal("That cell is already occupied.", ex.Message);
        Assert.Equal(Player.O, game.CurrentPlayer);
        Assert.Single(game.MoveHistory);
    }

    [Fact]
    public void Play_WrongPlayer_Throws()
    {
        var game = new Game(GameMode.TwoPlayer);

        var ex = Assert.Throws<GameException>(() => game.Play(Player.O, 0, 0, Computer));

        Assert.Contains("X", ex.Message);
        Assert.Empty(game.MoveHistory);
    }

    [Fact]
    public void Play_AfterCompletion_Throws()
    {
        var game = WinForX();

        var ex = Assert.Throws<GameException>(() => game.Play(Player.O, 2, 0, Computer));

        Assert.Equal("The game is already completed.", ex.Message);
    }

    [Fact]
    public void Play_RowWin_SetsWinnerAndWinningCells()
    {
        var game = new Game(GameMode.TwoPlayer);
        game.Play(Player.X, 0, 0, Computer);
        game.Play(Player.O, 1, 0, Computer);
        game.Play(Player.X, 0, 1, Computer);
        game.Play(Player.O, 1, 1, Computer);
        game.Play(Player.X, 0, 2, Computer);

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(Player.X, game.Winner);
        Assert.Equal(new[] { new CellPosition(0, 0), new CellPosition(0, 1), new CellPosition(0, 2) }, game.WinningCells);
    }

    [Fact]
    public void Play_ColumnWin_SetsWinnerAndWinningCells()
    {
        var game = new Game(GameMode.TwoPlayer);
        game.Play(Player.X, 0, 2, Computer);
        game.Play(Player.O, 0, 0, Computer);
        game.Play(Player.X, 1, 2, Computer);
        game.Play(Player.O, 1, 0, Computer);
        game.Play(Player.X, 2, 2, Computer);

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(Player.X, game.Winner);
        Assert.Equal(new[] { new CellPosition(0, 2), new CellPosition(1, 2), new CellPosition(2, 2) }, game.WinningCells);
    }

    [Fact]
    public void Play_DiagonalWin_SetsWinnerAndWinningCells()
    {
        var game = new Game(GameMode.TwoPlayer);
        game.Play(Player.X, 0, 0, Computer);
        game.Play(Player.O, 0, 1, Computer);
        game.Play(Player.X, 1, 1, Computer);
        game.Play(Player.O, 0, 2, Computer);
        game.Play(Player.X, 2, 2, Computer);

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(Player.X, game.Winner);
        Assert.Equal(new[] { new CellPosition(0, 0), new CellPosition(1, 1), new CellPosition(2, 2) }, game.WinningCells);
    }

    [Fact]
    public void Play_AntiDiagonalWin_SetsWinner()
    {
        var game = new Game(GameMode.TwoPlayer);
        game.Play(Player.X, 0, 2, Computer);
        game.Play(Player.O, 0, 0, Computer);
        game.Play(Player.X, 1, 1, Computer);
        game.Play(Player.O, 1, 0, Computer);
        game.Play(Player.X, 2, 0, Computer);

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(Player.X, game.Winner);
        Assert.Equal(new[] { new CellPosition(0, 2), new CellPosition(1, 1), new CellPosition(2, 0) }, game.WinningCells);
    }

    [Fact]
    public void Play_FullBoardWithoutWinner_IsDraw()
    {
        var game = PlayDraw();

        Assert.Equal(GameStatus.Draw, game.Status);
        Assert.Null(game.Winner);
        Assert.Empty(game.WinningCells);
        Assert.Equal(9, game.MoveHistory.Count);
    }

    [Fact]
    public void Reset_ClearsBoardHistoryAndStatus_KeepsIdAndMode()
    {
        var game = WinForX();
        var id = game.Id;

        game.Reset();

        Assert.Equal(id, game.Id);
        Assert.Equal(GameMode.TwoPlayer, game.Mode);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Null(game.Winner);
        Assert.Empty(game.MoveHistory);
        Assert.Empty(game.WinningCells);
        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.False(game.CanUndo);
        Assert.All(game.BoardSnapshot(), cell => Assert.Null(cell));
    }

    [Fact]
    public void Undo_TwoPlayerMode_RemovesOnlyLastMove()
    {
        var game = new Game(GameMode.TwoPlayer);
        game.Play(Player.X, 0, 0, Computer);
        game.Play(Player.O, 1, 1, Computer);

        game.Undo();

        Assert.Equal(Player.X, game.BoardAsGrid()[0][0]);
        Assert.Null(game.BoardAsGrid()[1][1]);
        Assert.Equal(Player.O, game.CurrentPlayer);
        Assert.Single(game.MoveHistory);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Fact]
    public void Undo_ComputerMode_RemovesComputerAndHumanMovePair()
    {
        var game = new Game(GameMode.Computer);
        game.Play(Player.X, 0, 0, Computer);

        Assert.Equal(2, game.MoveHistory.Count);
        Assert.Equal(Player.O, game.MoveHistory[1].Player);

        game.Undo();

        Assert.Empty(game.MoveHistory);
        Assert.Null(game.BoardAsGrid()[0][0]);
        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Fact]
    public void Undo_AfterHumanWinInComputerMode_RemovesWinningMoveAndPreviousComputerMove()
    {
        var game = PlayComputerModeXWin();

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(Player.X, game.Winner);

        game.Undo();

        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Null(game.Winner);
        Assert.Empty(game.WinningCells);
        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Equal(Player.X, game.MoveHistory[^1].Player);
    }

    [Fact]
    public void Undo_WithNoMoves_Throws()
    {
        var game = new Game(GameMode.TwoPlayer);

        var ex = Assert.Throws<GameException>(game.Undo);

        Assert.Equal("There are no moves to undo.", ex.Message);
    }

    [Fact]
    public void Play_ComputerMode_ComputerPlaysAutomatically()
    {
        var game = new Game(GameMode.Computer);

        game.Play(Player.X, 0, 0, Computer);

        Assert.Equal(2, game.MoveHistory.Count);
        Assert.Equal(Player.X, game.MoveHistory[0].Player);
        Assert.Equal(Player.O, game.MoveHistory[1].Player);
        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Equal(Player.O, game.BoardAsGrid()[1][1]);
    }

    [Fact]
    public void Play_ComputerModeHumanPlaysO_Throws()
    {
        var game = new Game(GameMode.Computer);

        var ex = Assert.Throws<GameException>(() => game.Play(Player.O, 0, 0, Computer));

        Assert.Contains("computer", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Play_ComputerDoesNotMoveAfterGameAlreadyComplete()
    {
        var game = new Game(GameMode.Computer);
        game.Play(Player.X, 0, 0, Computer);
        game.Play(Player.X, 2, 2, Computer);
        game.Play(Player.X, 2, 0, Computer);
        var historyBeforeWinningMove = game.MoveHistory.Count;

        game.Play(Player.X, 2, 1, Computer);

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(Player.X, game.Winner);
        Assert.Equal(historyBeforeWinningMove + 1, game.MoveHistory.Count);
        Assert.Equal(Player.X, game.MoveHistory[^1].Player);
    }

    private static Game WinForX()
    {
        var game = new Game(GameMode.TwoPlayer);
        game.Play(Player.X, 0, 0, Computer);
        game.Play(Player.O, 1, 0, Computer);
        game.Play(Player.X, 0, 1, Computer);
        game.Play(Player.O, 1, 1, Computer);
        game.Play(Player.X, 0, 2, Computer);
        return game;
    }

    private static Game PlayDraw()
    {
        var game = new Game(GameMode.TwoPlayer);
        game.Play(Player.X, 0, 0, Computer);
        game.Play(Player.O, 0, 1, Computer);
        game.Play(Player.X, 0, 2, Computer);
        game.Play(Player.O, 1, 1, Computer);
        game.Play(Player.X, 1, 0, Computer);
        game.Play(Player.O, 1, 2, Computer);
        game.Play(Player.X, 2, 1, Computer);
        game.Play(Player.O, 2, 0, Computer);
        game.Play(Player.X, 2, 2, Computer);
        return game;
    }

    private static Game PlayComputerModeXWin()
    {
        var game = new Game(GameMode.Computer);
        game.Play(Player.X, 0, 0, Computer);
        game.Play(Player.X, 2, 2, Computer);
        game.Play(Player.X, 2, 0, Computer);
        game.Play(Player.X, 2, 1, Computer);
        return game;
    }
}
