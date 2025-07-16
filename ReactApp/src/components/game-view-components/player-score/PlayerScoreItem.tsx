import { BsPaletteFill, BsShieldShaded } from "react-icons/bs";
import { PlayerScore } from "../../../interfaces/PlayerScore";
import { useAppSelector } from "../../../redux/hooks";

interface PlayerScoreItemProps {
  index: number;
  playerScore: PlayerScore;
  displayPoints: boolean;
  displayIndex: boolean;
}

const PlayerScoreItem = ({
  index,
  playerScore,
  displayPoints,
  displayIndex,
}: PlayerScoreItemProps) => {
  const player = useAppSelector((state) => state.player);
  const gameState = useAppSelector((state) => state.gameState);

  const isCorrect = gameState.correctGuessPlayerUsernames?.includes(playerScore.username);
  const isDrawing = gameState.drawingPlayerUsername === playerScore.username;

  const itemClass = `list-group-item d-flex justify-content-between align-items-center ${
    isCorrect ? "bg-custom-lime" : isDrawing ? "bg-light" : "bg-white"
  }`;

  return (
    <li className={itemClass} style={{ overflowWrap: "break-word" }}>
      {displayIndex && "#" + (index + 1)}
      <div className="d-flex flex-column align-items-center">
        <span className={player.username === playerScore.username ? "text-warning" : "text-dark"}>
          <b>{playerScore.username} </b>
          {gameState.hostPlayerUsername === playerScore.username && <BsShieldShaded />}
          {isDrawing && <BsPaletteFill />}
        </span>
        {displayPoints && (
          <span className="badge rounded-pill bg-dark mt-2">
            {playerScore.score} points
          </span>
        )}
      </div>
      <div></div>
    </li>
  );
};

export default PlayerScoreItem;