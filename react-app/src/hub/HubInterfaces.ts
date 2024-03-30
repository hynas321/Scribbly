import { PlayerScore } from "../types/PlayerScore";

export interface JoinGameResponse {
  gameHash: string;
  playerScores: PlayerScore[];
}