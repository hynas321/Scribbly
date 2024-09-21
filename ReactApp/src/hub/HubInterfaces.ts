import { PlayerScore } from "../interfaces/PlayerScore";

export interface JoinGameResponse {
  gameHash: string;
  playerScores: PlayerScore[];
}