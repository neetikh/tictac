namespace TicTacToe.Domain;

public interface IGameSessionService
{
    GameSnapshot Create(GameMode mode);
    GameSnapshot Get(Guid id);
    GameSnapshot MakeMove(Guid id, Player player, int row, int col);
    GameSnapshot Undo(Guid id);
    GameSnapshot Reset(Guid id);
    ScoreboardSnapshot GetScoreboard();
    ScoreboardSnapshot ResetScoreboard();
}

public sealed class GameSessionService : IGameSessionService
{
    private readonly Dictionary<Guid, Game> _games = [];
    private readonly ScoreboardState _scoreboard = new();
    private readonly IComputerPlayer _computer;
    private readonly object _sync = new();

    public GameSessionService() : this(new ComputerPlayer())
    {
    }

    public GameSessionService(IComputerPlayer computer)
    {
        _computer = computer;
    }

    public GameSnapshot Create(GameMode mode)
    {
        lock (_sync)
        {
            var game = new Game(mode);
            _games[game.Id] = game;
            return Snapshot(game);
        }
    }

    public GameSnapshot Get(Guid id)
    {
        lock (_sync)
        {
            return Snapshot(GetGame(id));
        }
    }

    public GameSnapshot MakeMove(Guid id, Player player, int row, int col)
    {
        lock (_sync)
        {
            var game = GetGame(id);
            var previousStatus = game.Status;
            var previousWinner = game.Winner;
            game.Play(player, row, col, _computer);
            ApplyScoreTransition(game, previousStatus, previousWinner);
            return Snapshot(game);
        }
    }

    public GameSnapshot Undo(Guid id)
    {
        lock (_sync)
        {
            var game = GetGame(id);
            var previousStatus = game.Status;
            var previousWinner = game.Winner;
            game.Undo();
            ApplyScoreTransition(game, previousStatus, previousWinner);
            return Snapshot(game);
        }
    }

    public GameSnapshot Reset(Guid id)
    {
        lock (_sync)
        {
            var game = GetGame(id);
            game.Reset();
            return Snapshot(game);
        }
    }

    public ScoreboardSnapshot GetScoreboard()
    {
        lock (_sync)
        {
            return _scoreboard.Snapshot();
        }
    }

    public ScoreboardSnapshot ResetScoreboard()
    {
        lock (_sync)
        {
            _scoreboard.Reset();
            foreach (var game in _games.Values)
            {
                game.ClearOutcomeCounted();
            }

            return _scoreboard.Snapshot();
        }
    }

    private Game GetGame(Guid id)
    {
        if (!_games.TryGetValue(id, out var game))
        {
            throw new GameNotFoundException(id);
        }

        return game;
    }

    private void ApplyScoreTransition(Game game, GameStatus previousStatus, Player? previousWinner)
    {
        var wasComplete = previousStatus != GameStatus.InProgress;
        var isComplete = game.Status != GameStatus.InProgress;

        if (!wasComplete && isComplete && !game.OutcomeCounted)
        {
            _scoreboard.Add(game.Status, game.Winner);
            game.MarkOutcomeCounted();
        }
        else if (wasComplete && !isComplete && game.OutcomeCounted)
        {
            _scoreboard.Subtract(previousStatus, previousWinner);
            game.ClearOutcomeCounted();
        }
    }

    private GameSnapshot Snapshot(Game game) => new(
        game.Id,
        game.BoardAsGrid(),
        game.CurrentPlayer,
        game.Mode,
        game.Status,
        game.Winner,
        game.WinningCells,
        game.MoveHistory.ToList(),
        game.CanUndo,
        _scoreboard.Snapshot());

    private sealed class ScoreboardState
    {
        public int XWins { get; private set; }
        public int OWins { get; private set; }
        public int Draws { get; private set; }

        public ScoreboardSnapshot Snapshot() => new(XWins, OWins, Draws);

        public void Reset()
        {
            XWins = 0;
            OWins = 0;
            Draws = 0;
        }

        public void Add(GameStatus status, Player? winner)
        {
            if (status == GameStatus.Draw)
            {
                Draws++;
            }
            else if (winner == Player.X)
            {
                XWins++;
            }
            else if (winner == Player.O)
            {
                OWins++;
            }
        }

        public void Subtract(GameStatus status, Player? winner)
        {
            if (status == GameStatus.Draw)
            {
                Draws = Math.Max(0, Draws - 1);
            }
            else if (winner == Player.X)
            {
                XWins = Math.Max(0, XWins - 1);
            }
            else if (winner == Player.O)
            {
                OWins = Math.Max(0, OWins - 1);
            }
        }
    }
}
