import { CompletedPlayerScore } from "../../http/HttpInterfaces";

export interface RankedPlayerScore extends CompletedPlayerScore {
  place: number;
}

export function rankScores(scores: CompletedPlayerScore[]): RankedPlayerScore[] {
  const sorted = [...scores].sort((left, right) => right.score - left.score);
  let place = 0;
  return sorted.map((player, index) => {
    if (index === 0 || player.score !== sorted[index - 1].score) {
      place = index + 1;
    }
    return { ...player, place };
  });
}
