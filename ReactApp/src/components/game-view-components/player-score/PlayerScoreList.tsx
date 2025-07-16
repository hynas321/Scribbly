import { PlayerScore } from "../../../interfaces/PlayerScore";
import PlayerScoreItem from "./PlayerScoreItem";

interface PlayerScoreListProps {
  title: string;
  playerScores: PlayerScore[];
  displayPoints: boolean;
  displayIndex: boolean;
}

const PlayerScoreList = ({
  title,
  playerScores,
  displayPoints,
  displayIndex,
}: PlayerScoreListProps) => {
  return (
    <ul className="list-group">
      <li className="list-group-item justify-content-between align-items-center">
        <b>{title}</b>
      </li>
      {playerScores.map((playerScore, index) => (
        <PlayerScoreItem
          key={index}
          index={index}
          playerScore={playerScore}
          displayPoints={displayPoints}
          displayIndex={displayIndex}
        />
      ))}
    </ul>
  );
};

export default PlayerScoreList;