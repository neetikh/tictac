using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using TicTacToe.Domain;

namespace TicTacToe.Api.Tests;

public class GamesApiTests : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public GamesApiTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task CreateGame_ReturnsInitialState()
    {
        var game = await CreateGameAsync(GameMode.TwoPlayer);

        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Equal(GameMode.TwoPlayer, game.Mode);
        Assert.False(game.CanUndo);
        Assert.Equal(3, game.Board.Length);
        Assert.All(game.Board.SelectMany(row => row), cell => Assert.Null(cell));
    }

    [Fact]
    public async Task SubmitMove_ValidMove_UpdatesBoardAndHistory()
    {
        var game = await CreateGameAsync(GameMode.TwoPlayer);

        var updated = await MoveAsync(game.Id, Player.X, 0, 0);

        Assert.Equal(Player.X, updated.Board[0][0]);
        Assert.Equal(Player.O, updated.CurrentPlayer);
        Assert.True(updated.CanUndo);
        Assert.Equal("Row 1, Column 1", updated.MoveHistory[0].Position);
    }

    [Fact]
    public async Task SubmitMove_OccupiedCell_Returns400()
    {
        var game = await CreateGameAsync(GameMode.TwoPlayer);
        await MoveAsync(game.Id, Player.X, 0, 0);

        var response = await _client.PostAsJsonAsync($"/api/games/{game.Id}/moves", new { player = "O", row = 0, col = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal("That cell is already occupied.", payload.GetProperty("error").GetString());
    }

    [Fact]
    public async Task SubmitMove_AfterWin_Returns400()
    {
        var game = await PlayXRowWinAsync();

        var response = await _client.PostAsJsonAsync($"/api/games/{game.Id}/moves", new { player = "O", row = 2, col = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Win_UpdatesScoreboardOnce()
    {
        var game = await PlayXRowWinAsync();

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(Player.X, game.Winner);
        Assert.Equal(3, game.WinningCells.Count);
        Assert.Equal(1, game.Scoreboard.XWins);

        var again = await GetGameAsync(game.Id);
        Assert.Equal(1, again.Scoreboard.XWins);
    }

    [Fact]
    public async Task ResetGame_ClearsBoardButKeepsScoreboard()
    {
        var game = await PlayXRowWinAsync();

        var reset = await PostAsync($"/api/games/{game.Id}/reset");

        Assert.Equal(GameStatus.InProgress, reset.Status);
        Assert.Empty(reset.MoveHistory);
        Assert.Equal(Player.X, reset.CurrentPlayer);
        Assert.Equal(1, reset.Scoreboard.XWins);
        Assert.False(reset.CanUndo);
    }

    [Fact]
    public async Task Undo_AfterWin_AdjustsScoreboard()
    {
        var game = await PlayXRowWinAsync();
        Assert.Equal(1, game.Scoreboard.XWins);

        var undone = await PostAsync($"/api/games/{game.Id}/undo");

        Assert.Equal(GameStatus.InProgress, undone.Status);
        Assert.Null(undone.Winner);
        Assert.Equal(0, undone.Scoreboard.XWins);
        Assert.Equal(Player.X, undone.CurrentPlayer);
    }

    [Fact]
    public async Task ComputerMode_MakesAutomaticReply()
    {
        var game = await CreateGameAsync(GameMode.Computer);

        var updated = await MoveAsync(game.Id, Player.X, 0, 0);

        Assert.Equal(2, updated.MoveHistory.Count);
        Assert.Equal(Player.O, updated.MoveHistory[1].Player);
        Assert.Equal(Player.O, updated.Board[1][1]);
    }

    [Fact]
    public async Task ResetScoreboard_ZerosCounts()
    {
        var game = await PlayXRowWinAsync();
        Assert.Equal(1, game.Scoreboard.XWins);

        var scoreboard = await _client.PostAsJsonAsync("/api/scoreboard/reset", new { });
        var body = await scoreboard.Content.ReadFromJsonAsync<ScoreboardSnapshot>(JsonOptions);

        Assert.Equal(0, body!.XWins);
        Assert.Equal(0, body.OWins);
        Assert.Equal(0, body.Draws);
    }

    [Fact]
    public async Task GetMissingGame_Returns404()
    {
        var response = await _client.GetAsync($"/api/games/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<GameSnapshot> CreateGameAsync(GameMode mode)
    {
        var response = await _client.PostAsJsonAsync("/api/games", new { mode = mode.ToString() });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GameSnapshot>(JsonOptions))!;
    }

    private async Task<GameSnapshot> MoveAsync(Guid id, Player player, int row, int column)
    {
        var response = await _client.PostAsJsonAsync(
            $"/api/games/{id}/moves",
            new { player = player.ToString(), row, column });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GameSnapshot>(JsonOptions))!;
    }

    private async Task<GameSnapshot> GetGameAsync(Guid id)
    {
        var response = await _client.GetAsync($"/api/games/{id}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GameSnapshot>(JsonOptions))!;
    }

    private async Task<GameSnapshot> PostAsync(string path)
    {
        var response = await _client.PostAsJsonAsync(path, new { });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GameSnapshot>(JsonOptions))!;
    }

    private async Task<GameSnapshot> PlayXRowWinAsync()
    {
        var game = await CreateGameAsync(GameMode.TwoPlayer);
        await MoveAsync(game.Id, Player.X, 0, 0);
        await MoveAsync(game.Id, Player.O, 1, 0);
        await MoveAsync(game.Id, Player.X, 0, 1);
        await MoveAsync(game.Id, Player.O, 1, 1);
        return await MoveAsync(game.Id, Player.X, 0, 2);
    }
}
