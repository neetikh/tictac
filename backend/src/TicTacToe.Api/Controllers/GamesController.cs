using Microsoft.AspNetCore.Mvc;
using TicTacToe.Domain;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/games")]
public sealed class GamesController(IGameSessionService games) : ControllerBase
{
    [HttpPost]
    public ActionResult<GameSnapshot> Create([FromBody] CreateGameRequest? request)
    {
        var mode = request?.Mode ?? GameMode.TwoPlayer;
        return Ok(games.Create(mode));
    }

    [HttpGet("{id:guid}")]
    public ActionResult<GameSnapshot> Get(Guid id) => Ok(games.Get(id));

    [HttpPost("{id:guid}/moves")]
    public ActionResult<GameSnapshot> Move(Guid id, [FromBody] MakeMoveRequest request)
        => Ok(games.MakeMove(id, request.Player, request.Row, request.Column));

    [HttpPost("{id:guid}/undo")]
    public ActionResult<GameSnapshot> Undo(Guid id) => Ok(games.Undo(id));

    [HttpPost("{id:guid}/reset")]
    public ActionResult<GameSnapshot> Reset(Guid id) => Ok(games.Reset(id));
}

[ApiController]
[Route("api/scoreboard")]
public sealed class ScoreboardController(IGameSessionService games) : ControllerBase
{
    [HttpGet]
    public ActionResult<ScoreboardSnapshot> Get() => Ok(games.GetScoreboard());

    [HttpPost("reset")]
    public ActionResult<ScoreboardSnapshot> Reset() => Ok(games.ResetScoreboard());
}

public sealed record CreateGameRequest(GameMode Mode);

public sealed record MakeMoveRequest(Player Player, int Row, int Column);
