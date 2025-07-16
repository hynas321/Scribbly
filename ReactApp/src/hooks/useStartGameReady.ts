import { useEffect, useState } from "react";
import { PlayerScore } from "../interfaces/PlayerScore";

export const useStartGameReady = (playerScores: PlayerScore[]) => {
  const [isReady, setIsReady] = useState(false);

  useEffect(() => {
    setIsReady(playerScores.length > 1);
  }, [playerScores]);

  return isReady;
};