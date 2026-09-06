using TicTacToe.Domain;

namespace TicTacToe.Domain.Tests;

public class WinDetectorTests
{
    [Fact]
    public void Find_RowWin_ReturnsWinnerAndCells()
    {
        var board = new Player?[9];
        board[0] = Player.X;
        board[1] = Player.X;
        board[2] = Player.X;

        var (winner, cells) = WinDetector.Find(board);

        Assert.Equal(Player.X, winner);
        Assert.Equal(new[] { new CellPosition(0, 0), new CellPosition(0, 1), new CellPosition(0, 2) }, cells);
    }

    [Fact]
    public void Find_NoWinner_ReturnsEmpty()
    {
        var board = new Player?[9];
        board[0] = Player.X;
        board[1] = Player.O;

        var (winner, cells) = WinDetector.Find(board);

        Assert.Null(winner);
        Assert.Empty(cells);
    }
}
