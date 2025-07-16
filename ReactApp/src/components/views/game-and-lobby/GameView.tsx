import { useEffect, useState, useContext } from "react";
import loading from "./../../../assets/loading.gif";
import { useNavigate, useLocation } from "react-router-dom";
import { useAppSelector } from "../../../redux/hooks";
import { PlayerScore } from "../../../redux/slices/player-score-slice";
import { useStartGameReady } from "../../../hooks/useStartGameReady";
import UrlHelper from "../../../utils/UrlHelper";
import { useGameHubEvents } from "../../../hooks/hub-events/useGameHubEvents";
import InGame from "./InGame";
import InLobby from "./InLobby";
import { ConnectionHubContext, LongRunningConnectionHubContext } from "../../../context/ConnectionHubContext";
import { useLobbyActions } from "../../../hooks/useLobbyActions";

function GameView() {
  const location = useLocation();
  const gameState = useAppSelector(state => state.gameState);
  const gameSettings = useAppSelector(state => state.gameSettings);
  const hub = useContext(ConnectionHubContext);
  const longRunningHub = useContext(LongRunningConnectionHubContext);

  const { fromViewNavigation } = location.state ?? false;
  const [playerScores, setPlayerScores] = useState<PlayerScore[]>([]);
  const [gameHash, setGameHash] = useState("");
  const [isPlayerHost, setIsPlayerHost] = useState(false);
  const [isGameDisplayed, setIsGameDisplayed] = useState(false);
  const [isGameFinished, setIsGameFinished] = useState(false);

  useEffect(() => {
    setGameHash(UrlHelper.getGameHash(window.location.href));
  }, []);

  useGameHubEvents(
    hub,
    gameHash,
    setPlayerScores,
    setIsPlayerHost,
    setIsGameDisplayed,
    fromViewNavigation,
    isGameFinished,
    setIsGameFinished
  );

  const isStartGameButtonActive = useStartGameReady(playerScores);
  const { handleLeaveGame } = useLobbyActions(hub, longRunningHub, gameHash, gameSettings);

  return !isGameDisplayed ? (
    <div className="d-flex justify-content-center align-items-center mt-4">
      <img src={loading} alt="Loading" className="w-30 h-30 img-fluid" />
    </div>
  ) : gameState.isGameStarted ? (
    <InGame
      playerScores={playerScores}
      onLeave={handleLeaveGame}
    />
  ) : (
    <InLobby
      playerScores={playerScores}
      isPlayerHost={isPlayerHost}
      isStartGameButtonActive={isStartGameButtonActive}
      onLeave={handleLeaveGame}
      gameHash={gameHash}
    />
  );
}

export default GameView;
