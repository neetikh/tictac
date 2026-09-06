using TicTacToe.Domain;

namespace TicTacToe.Domain.Tests;

public class GameSessionServiceTests
{
    [Fact]
    public void MakeMove_UpdatesScoreboardOnceWhenGameIsWon()
    {
        var service = new GameSessionService();
        var game = PlayXWin(service);

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(1, game.Scoreboard.XWins);
        Assert.Equal(0, game.Scoreboard.OWins);
        Assert.Equal(0, game.Scoreboard.Draws);
        Assert.Equal(1, service.GetScoreboard().XWins);
    }

    [Fact]
    public void MakeMove_UpdatesScoreboardOnceWhenGameIsDrawn()
    {
        var service = new GameSessionService();
        var game = PlayDraw(service);

        Assert.Equal(GameStatus.Draw, game.Status);
        Assert.Equal(1, game.Scoreboard.Draws);
        Assert.Equal(0, game.Scoreboard.XWins);
    }

    [Fact]
    public void Reset_KeepsScoreboardUnchanged()
    {
        var service = new GameSessionService();
        var game = PlayXWin(service);

        var reset = service.Reset(game.Id);

        Assert.Equal(GameStatus.InProgress, reset.Status);
        Assert.Empty(reset.MoveHistory);
        Assert.Equal(Player.X, reset.CurrentPlayer);
        Assert.Equal(1, reset.Scoreboard.XWins);
        Assert.False(reset.CanUndo);
    }

    [Fact]
    public void Undo_AfterWin_AdjustsScoreboard()
    {
        var service = new GameSessionService();
        var game = PlayXWin(service);

        var undone = service.Undo(game.Id);

        Assert.Equal(GameStatus.InProgress, undone.Status);
        Assert.Equal(0, undone.Scoreboard.XWins);
        Assert.Equal(Player.X, undone.CurrentPlayer);
    }

    [Fact]
    public void ResetScoreboard_ZerosTotals()
    {
        var service = new GameSessionService();
        var game = PlayXWin(service);

        var scoreboard = service.ResetScoreboard();
        var current = service.Get(game.Id);

        Assert.Equal(0, scoreboard.XWins);
        Assert.Equal(0, scoreboard.OWins);
        Assert.Equal(0, scoreboard.Draws);
        Assert.Equal(0, current.Scoreboard.XWins);
        Assert.Equal(GameStatus.Won, current.Status);
    }

    [Fact]
    public void Create_ComputerMode_AutoPlaysOAfterX()
    {
        var service = new GameSessionService();
        var created = service.Create(GameMode.Computer);

        var afterMove = service.MakeMove(created.Id, Player.X, 0, 0);

        Assert.Equal(2, afterMove.MoveHistory.Count);
        Assert.Equal(Player.X, afterMove.MoveHistory[0].Player);
        Assert.Equal(Player.O, afterMove.MoveHistory[1].Player);
        Assert.Equal(Player.X, afterMove.CurrentPlayer);
        Assert.Equal(4, afterMove.MoveHistory[1].Row * 3 + afterMove.MoveHistory[1].Column);
    }

    [Fact]
    public void Get_UnknownGame_ThrowsNotFound()
    {
        var service = new GameSessionService();

        Assert.Throws<GameNotFoundException>(() => service.Get(Guid.NewGuid()));
    }

    private static GameSnapshot PlayXWin(GameSessionService service)
    {
        var game = service.Create(GameMode.TwoPlayer);
        service.MakeMove(game.Id, Player.X, 0, 0);
        service.MakeMove(game.Id, Player.O, 1, 0);
        service.MakeMove(game.Id, Player.X, 0, 1);
        service.MakeMove(game.Id, Player.O, 1, 1);
        return service.MakeMove(game.Id, Player.X, 0, 2);
    }

    private static GameSnapshot PlayDraw(GameSessionService service)
    {
        var game = service.Create(GameMode.TwoPlayer);
        service.MakeMove(game.Id, Player.X, 0, 0);
        service.MakeMove(game.Id, Player.O, 0, 1);
        service.MakeMove(game.Id, Player.X, 0, 2);
        service.MakeMove(game.Id, Player.O, 1, 1);
        service.MakeMove(game.Id, Player.X, 1, 0);
        service.MakeMove(game.Id, Player.O, 1, 2);
        service.MakeMove(game.Id, Player.X, 2, 1);
        service.MakeMove(game.Id, Player.O, 2, 0);
        return service.MakeMove(game.Id, Player.X, 2, 2);
    }
}
