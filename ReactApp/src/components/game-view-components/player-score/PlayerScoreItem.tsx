import { useContext } from "react";
import { useParams } from "react-router-dom";
import { BsPaletteFill, BsShieldShaded, BsXCircle } from "react-icons/bs";
import { PlayerScore } from "../../../interfaces/PlayerScore";
import { useAppSelector } from "../../../redux/hooks";
import { ConnectionHubContext } from "../../../context/ConnectionHubContext";
import HubEvents from "../../../hub/HubMessages";
import { reportRealtimeError } from "../../../utils/reportRealtimeError";
import { useTranslation } from "../../../i18n/I18nContext";

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
  const { t } = useTranslation();
  const hub = useContext(ConnectionHubContext);
  const { gameHash = "" } = useParams();
  const player = useAppSelector((state) => state.player);
  const gameState = useAppSelector((state) => state.gameState);

  const isCorrect = gameState.correctGuessPlayerUsernames?.includes(playerScore.username);
  const isDrawing = gameState.drawingPlayerUsername === playerScore.username;
  const canKick =
    player.username === gameState.hostPlayerUsername &&
    playerScore.username !== gameState.hostPlayerUsername;

  const kick = async () => {
    try {
      await hub.invoke(HubEvents.kickPlayer, gameHash, playerScore.username);
    } catch {
      reportRealtimeError(t("error.action.kickPlayer"));
    }
  };

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
            {t("common.scorePoints", { score: playerScore.score })}
          </span>
        )}
      </div>
      <div>
        {canKick && (
          <button className="btn btn-sm btn-outline-danger" onClick={() => void kick()}>
            <BsXCircle />
          </button>
        )}
      </div>
    </li>
  );
};

export default PlayerScoreItem;
