import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { AppComponent } from './app.component';
import { GameApiService } from './game-api.service';
import { GameState } from './models';

describe('AppComponent', () => {
  const game: GameState = {
    id: 'game-1',
    board: [
      [null, null, null],
      [null, null, null],
      [null, null, null]
    ],
    currentPlayer: 'X',
    mode: 'TwoPlayer',
    status: 'InProgress',
    winner: null,
    winningCells: [],
    moveHistory: [],
    canUndo: false,
    scoreboard: { xWins: 0, oWins: 0, draws: 0 }
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [
        provideHttpClient(),
        {
          provide: GameApiService,
          useValue: {
            create: () => of(game),
            move: () => of(game),
            undo: () => of(game),
            reset: () => of(game),
            resetScoreboard: () => of(game.scoreboard)
          }
        }
      ]
    }).compileComponents();
  });

  it('creates a game on init and renders the board', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;

    expect(fixture.componentInstance.game?.id).toBe('game-1');
    expect(compiled.querySelectorAll('.cell').length).toBe(9);
    expect(compiled.textContent).toContain("Player X's turn.");
    expect(compiled.textContent).toContain('Scoreboard');
    expect(compiled.textContent).toContain('Move History');
  });

  it('highlights winning cells from backend state', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    fixture.componentInstance.game = {
      ...game,
      status: 'Won',
      winner: 'X',
      winningCells: [{ row: 0, column: 0 }, { row: 0, column: 1 }, { row: 0, column: 2 }],
      board: [
        ['X', 'X', 'X'],
        ['O', 'O', null],
        [null, null, null]
      ]
    };
    fixture.detectChanges();

    expect(fixture.componentInstance.isWinningCell(0, 1)).toBeTrue();
    expect(fixture.componentInstance.isWinningCell(1, 0)).toBeFalse();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Player X wins.');
  });
});
