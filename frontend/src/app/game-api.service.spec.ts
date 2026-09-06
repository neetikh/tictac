import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { GameApiService } from './game-api.service';
import { GameState } from './models';

describe('GameApiService', () => {
  let service: GameApiService;
  let http: HttpTestingController;

  const game: GameState = {
    id: 'abc',
    board: [[null, null, null], [null, null, null], [null, null, null]],
    currentPlayer: 'X',
    mode: 'TwoPlayer',
    status: 'InProgress',
    winner: null,
    winningCells: [],
    moveHistory: [],
    scoreboard: { xWins: 0, oWins: 0, draws: 0 },
    canUndo: false
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(GameApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('create posts the selected mode', () => {
    service.create('Computer').subscribe(result => {
      expect(result.mode).toBe('Computer');
    });

    const req = http.expectOne('/api/games');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ mode: 'Computer' });
    req.flush({ ...game, mode: 'Computer' });
  });

  it('move posts player and cell coordinates', () => {
    service.move('abc', 'X', 1, 2).subscribe();

    const req = http.expectOne('/api/games/abc/moves');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ player: 'X', row: 1, column: 2 });
    req.flush(game);
  });
});
