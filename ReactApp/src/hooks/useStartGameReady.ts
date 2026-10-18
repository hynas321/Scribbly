import { PlayerScore } from "../interfaces/PlayerScore";
export const useStartGameReady = (playerScores: PlayerScore[]) => playerScores.length >= 2;
