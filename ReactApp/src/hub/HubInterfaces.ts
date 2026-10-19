import { PlayerScore } from "../interfaces/PlayerScore";
import { GameSettings } from "../redux/slices/game-settings-slice";

export interface GameClientState {
  currentDrawingTimeSeconds: number;
  currentRound: number;
  hiddenPromptText: string;
  promptCategory: string;
  promptKind: string;
  drawingPlayerUsername: string;
  hostPlayerUsername: string;
  isGameStarted: boolean;
  isTimerVisible: boolean;
  correctGuessPlayerUsernames: string[];
  canvasEpoch: number;
}
export interface RoomSnapshot {
  gameHash: string;
  username: string;
  isHost: boolean;
  settings: GameSettings;
  state: GameClientState;
  scores: PlayerScore[];
  playerCount: number;
  maxPlayers: number;
}
export interface PromptView {
  text: string;
  category: string;
  kind: "word" | "phrase" | "saying" | string;
  isDrawingPlayer: boolean;
}
export interface CanvasStrokeChunk {
  epoch: number;
  strokeId: string;
  sequence: number;
  color: string;
  thickness: number;
  coordinates: number[];
}
export interface CanvasStrokeSnapshot {
  strokeId: string;
  color: string;
  thickness: number;
  coordinates: number[];
}
export interface CanvasSnapshot {
  epoch: number;
  revision: number;
  strokes: CanvasStrokeSnapshot[];
}
export type CanvasCommandStatus =
  | "applied"
  | "duplicate"
  | "gameNotFound"
  | "notAuthorized"
  | "waitingForPlayers"
  | "wrongEpoch"
  | "outOfOrder"
  | "limitExceeded"
  | "invalidStroke"
  | "nothingToUndo";
export interface CanvasCommandResult {
  status: CanvasCommandStatus;
  removedStrokeId?: string | null;
  epoch?: number | null;
  revision?: number | null;
}
