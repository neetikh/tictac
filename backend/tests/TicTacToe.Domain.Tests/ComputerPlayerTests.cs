using TicTacToe.Domain;

namespace TicTacToe.Domain.Tests;

public class ComputerPlayerTests
{
    private readonly ComputerPlayer _computer = new();

    [Fact]
    public void ChooseMove_WhenOCanWin_PlaysWinningMove()
    {
        var board = EmptyBoard();
        board[0] = Player.O;
        board[1] = Player.O;
        board[3] = Player.X;
        board[4] = Player.X;

        var move = _computer.ChooseMove(board);

        Assert.Equal(2, move);
    }

    [Fact]
    public void ChooseMove_WhenXCanWinNext_BlocksX()
    {
        var board = EmptyBoard();
        board[0] = Player.X;
        board[1] = Player.X;
        board[4] = Player.O;

        var move = _computer.ChooseMove(board);

        Assert.Equal(2, move);
    }

    [Fact]
    public void ChooseMove_TakesCenterIfAvailable()
    {
        var board = EmptyBoard();
        board[0] = Player.X;

        var move = _computer.ChooseMove(board);

        Assert.Equal(4, move);
    }

    [Fact]
    public void ChooseMove_TakesCornerWhenCenterTaken()
    {
        var board = EmptyBoard();
        board[4] = Player.X;

        var move = _computer.ChooseMove(board);

        Assert.Equal(0, move);
    }

    [Fact]
    public void ChooseMove_TakesAnyAvailableCellWhenCornersAndCenterAreGone()
    {
        var board = EmptyBoard();
        board[4] = Player.X;
        board[0] = Player.O;
        board[2] = Player.X;
        board[6] = Player.O;
        board[8] = Player.X;

        var move = _computer.ChooseMove(board);

        Assert.Null(board[move]);
        Assert.DoesNotContain(move, new[] { 0, 2, 4, 6, 8 });
    }

    [Fact]
    public void ChooseMove_PrefersWinningOverBlocking()
    {
        var board = EmptyBoard();
        board[0] = Player.O;
        board[1] = Player.O;
        board[6] = Player.X;
        board[7] = Player.X;

        var move = _computer.ChooseMove(board);

        Assert.Equal(2, move);
    }

    private static Player?[] EmptyBoard() => new Player?[9];
}
