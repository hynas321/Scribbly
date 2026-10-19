import { useContext, useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import loading from "../../../assets/loading.gif";
import { useAppSelector } from "../../../redux/hooks";
import { useStartGameReady } from "../../../hooks/useStartGameReady";
import { useRoomSession } from "../../../features/game-session/useRoomSession";
import InGame from "./InGame";
import InLobby from "./InLobby";
import { ConnectionHubContext } from "../../../context/ConnectionHubContext";
import { useLobbyActions } from "../../../hooks/useLobbyActions";
import { useTranslation } from "../../../i18n/I18nContext";

function GameView() {
  const { t } = useTranslation();
  const gameState = useAppSelector((state) => state.gameState);
  const gameSettings = useAppSelector((state) => state.gameSettings);
  const hub = useContext(ConnectionHubContext);
  const { gameHash = "" } = useParams();
  const { playerScores, isPlayerHost, isGameDisplayed } = useRoomSession(hub, gameHash);
  const canStart = useStartGameReady(playerScores);
  const { handleLeaveGame } = useLobbyActions(hub, gameHash, gameSettings);
  const [minLoadingElapsed, setMinLoadingElapsed] = useState(false);

  useEffect(() => {
    const timer = window.setTimeout(() => setMinLoadingElapsed(true), 1000);
    return () => window.clearTimeout(timer);
  }, []);

  return (
    <>
      {!isGameDisplayed || !minLoadingElapsed ? (
        <div className="d-flex justify-content-center align-items-center mt-4">
          <img src={loading} alt={t("app.loading")} className="w-30 h-30 img-fluid" />
        </div>
      ) : gameState.isGameStarted ? (
        <InGame playerScores={playerScores} onLeave={handleLeaveGame} />
      ) : (
        <InLobby
          playerScores={playerScores}
          isPlayerHost={isPlayerHost}
          isStartGameButtonActive={canStart}
          onLeave={handleLeaveGame}
          gameHash={gameHash}
        />
      )}
    </>
  );
}
export default GameView;
