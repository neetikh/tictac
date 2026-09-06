import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { GameApiService } from './game-api.service';
import { GameMode, GameState, Player } from './models';

@Component({
  selector: 'app-root',
  imports: [],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  private readonly api = inject(GameApiService);

  readonly rows = [0, 1, 2];
  game: GameState | null = null;
  error: string | null = null;
  busy = false;

  ngOnInit(): void {
    this.startGame('TwoPlayer');
  }

  startGame(mode: GameMode): void {
    this.run(this.api.create(mode));
  }

  play(row: number, col: number): void {
    if (!this.game || !this.canPlay(row, col)) {
      return;
    }

    this.run(this.api.move(this.game.id, this.game.currentPlayer, row, col));
  }

  undo(): void {
    if (!this.game?.canUndo) {
      return;
    }

    this.run(this.api.undo(this.game.id));
  }

  resetGame(): void {
    if (!this.game) {
      return;
    }

    this.run(this.api.reset(this.game.id));
  }

  resetScoreboard(): void {
    if (!this.game) {
      return;
    }

    this.busy = true;
    this.api.resetScoreboard().subscribe({
      next: (scoreboard) => {
        this.game = { ...this.game!, scoreboard };
        this.error = null;
        this.busy = false;
      },
      error: (err) => this.handleError(err)
    });
  }

  cell(row: number, col: number): string {
    return this.game?.board[row][col] ?? '';
  }

  canPlay(row: number, col: number): boolean {
    return !!this.game
      && this.game.status === 'InProgress'
      && !this.game.board[row][col]
      && !this.busy;
  }

  isWinningCell(row: number, col: number): boolean {
    return !!this.game?.winningCells.some((cell) => cell.row === row && cell.column === col);
  }

  modeLabel(mode: GameMode): string {
    return mode === 'Computer' ? 'Play Against Computer' : 'Two Player';
  }

  statusMessage(): string {
    if (!this.game) {
      return 'Starting a new game…';
    }

    if (this.game.status === 'Won') {
      return this.game.mode === 'Computer' && this.game.winner === 'O'
        ? 'Computer (O) wins.'
        : `Player ${this.game.winner} wins.`;
    }

    if (this.game.status === 'Draw') {
      return 'The game is a draw.';
    }

    if (this.game.mode === 'Computer') {
      return this.game.currentPlayer === 'X'
        ? 'Your turn — you are X.'
        : "Computer's turn.";
    }

    return `Player ${this.game.currentPlayer}'s turn.`;
  }

  positionLabel(row: number, col: number): string {
    return `Row ${row + 1}, Column ${col + 1}`;
  }

  playerMark(player: Player): string {
    return player;
  }

  private run(request: ReturnType<GameApiService['create']>): void {
    this.busy = true;
    request.subscribe({
      next: (game) => {
        this.game = game;
        this.error = null;
        this.busy = false;
      },
      error: (err) => this.handleError(err)
    });
  }

  private handleError(err: HttpErrorResponse): void {
    this.busy = false;
    this.error = err.error?.error ?? 'Unable to reach the game server. Is the API running on port 5080?';
  }
}
