# Tic Tac Toe

Browser-based Tic Tac Toe with an Angular frontend and a .NET Web API backend. The backend is the source of truth for the board, turn, win/draw status, move history, and session scoreboard.

## Tech stack

- Frontend: Angular 19 + TypeScript
- Backend: ASP.NET Core Web API (`net10.0`)
- API style: REST
- Storage: in-memory (process lifetime)
- Tests: xUnit (backend), Jasmine/Karma (frontend)

## Features

- 3×3 board with locked cells after a mark is placed
- Two Player mode and Play Against Computer (human is X, computer is O)
- Turn display, win detection (row/column/diagonal), draw detection
- Winning cells highlighted in the UI
- Move history for the current game
- Undo Last Move (one move in Two Player; human+computer pair in Computer mode)
- Session scoreboard for X wins, O wins, and draws
- Reset Game (clears the board, keeps the scoreboard)
- Reset Scoreboard (zeros the session totals)

## How to run locally

Use two terminals. The Angular dev server proxies `/api` to the backend.

### Backend

Requires the .NET 10 SDK.

```bash
cd backend
dotnet test
dotnet run --project src/TicTacToe.Api/TicTacToe.Api.csproj --urls http://localhost:5080
```

API base URL: `http://localhost:5080`

OpenAPI document: `http://localhost:5080/openapi/v1.json`

### Frontend

Requires Node.js 20+ (22 recommended).

```bash
cd frontend
npm install
npm start
```

UI: `http://localhost:4200`

## API contract

| Method | Endpoint | Purpose |
|--------|----------|---------|
| `POST` | `/api/games` | Create a game. Body: `{ "mode": "TwoPlayer" \| "Computer" }` |
| `GET` | `/api/games/{id}` | Get current game state |
| `POST` | `/api/games/{id}/moves` | Submit a move. Body: `{ "player": "X" \| "O", "row": 0-2, "column": 0-2 }` |
| `POST` | `/api/games/{id}/undo` | Undo according to game mode |
| `POST` | `/api/games/{id}/reset` | Reset the current game, keep scoreboard |
| `GET` | `/api/scoreboard` | Get session scoreboard |
| `POST` | `/api/scoreboard/reset` | Reset session scoreboard |

### Game state response

```json
{
  "id": "guid",
  "board": [["X", null, "O"], [null, "X", null], [null, null, null]],
  "currentPlayer": "X",
  "mode": "TwoPlayer",
  "status": "InProgress",
  "winner": null,
  "winningCells": [],
  "moveHistory": [{ "moveNumber": 1, "player": "X", "row": 0, "column": 0, "position": "Row 1, Column 1" }],
  "canUndo": true,
  "scoreboard": { "xWins": 0, "oWins": 0, "draws": 0 }
}
```

`status` is `InProgress`, `Won`, or `Draw`. Invalid moves return `400` with `{ "error": "..." }`. Unknown games return `404`.

In Computer mode the backend applies O’s reply in the same `POST /moves` call after a valid X move, unless the game already ended.

## How to run tests

```bash
cd backend
dotnet test
```

```bash
cd frontend
npm test -- --watch=false --browsers=ChromeHeadless
```

Backend tests cover valid/invalid moves, turn switching, row/column/diagonal wins, draws, reset, undo in both modes, scoreboard updates, computer move selection, and moves after completion.

## Design decisions

- **Backend owns the rules.** The Angular app only renders the latest game state. Validation, win/draw detection, computer play, undo, and scoring all happen in `TicTacToe.Domain`.
- **In-memory session store.** Enough for a local review. Restarting the API clears games and scores.
- **Computer reply is server-side.** The UI never posts O moves in Computer mode, so a slow or double-clicked UI cannot desync the turn.
- **Deterministic computer policy** as specified: win, then block, then center, then first free corner, then first free cell.
- **Undo after completion is allowed (Option B).** If a finished game is undone, the scoreboard is decremented so it stays consistent with the restored in-progress state.

## Clarifications and assumptions

- Rows and columns are 0-based in the API and shown as 1-based in the UI (`Row 1, Column 1`).
- Changing mode creates a new game and leaves the scoreboard unchanged.
- Computer mode: human is always X; O is never accepted from the client.
- Scoreboard is process-wide (all games in this API instance), matching a local interview session.
- If a completed game is reset, the recorded result stays on the scoreboard and a new empty game starts.

## Known limitations

- State is not persisted across API restarts.
- One API process; not a multi-user hosted lobby.
- Computer play is the required heuristic, not a full minimax search, so a fork can still beat it.
- HTTPS is not required for local demo; the UI talks to `http://localhost:5080` through the Angular proxy.

## Future improvements

- Persist games and scores in SQLite
- Stronger computer opponent (minimax / difficulty levels)
- Multi-game rooms and reconnect
- Animation and move-by-move history replay
- Docker Compose for one-command local startup

## AI tools and prompt summary

Built with Cursor (Grok) from the Round 2 problem statement.

What the prompt asked for:

- Implement the attached spec as a runnable Angular + .NET app
- Put the solution in `git@github.com:neetikh/tictac.git`

What was generated and then reviewed:

- Domain engine (`Game`, `WinDetector`, `ComputerPlayer`, `GameSessionService`)
- REST controllers and exception middleware
- xUnit unit and API tests
- Angular board UI, history, scoreboard, and proxy config
- This README

What was chosen manually:

- Option B for undo-after-completion (adjust scoreboard)
- Computer move applied in the same POST as the human move
- In-memory storage instead of SQLite
- `net10.0` because that is the SDK installed on the build machine
