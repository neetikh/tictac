export type Player = 'X' | 'O';
export type GameMode = 'TwoPlayer' | 'Computer';
export type GameStatus = 'InProgress' | 'Won' | 'Draw';

export interface CellPosition {
  row: number;
  column: number;
}

export interface MoveRecord {
  moveNumber: number;
  player: Player;
  row: number;
  column: number;
  position: string;
}

export interface Scoreboard {
  xWins: number;
  oWins: number;
  draws: number;
}

export interface GameState {
  id: string;
  board: (Player | null)[][];
  currentPlayer: Player;
  mode: GameMode;
  status: GameStatus;
  winner: Player | null;
  winningCells: CellPosition[];
  moveHistory: MoveRecord[];
  canUndo: boolean;
  scoreboard: Scoreboard;
}
