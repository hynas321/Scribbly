import { useEffect, useState } from "react";
import Button from "../Button";
import GameSettingsBoard from "../game-view-components/GameSettingsBoard";
import { useAppSelector } from "../../redux/hooks";
import config from "../../../config.json";
import { useLocation, useNavigate } from "react-router-dom";
import PlayerScores from "../game-view-components/PlayerScores";
import Chat from "../game-view-components/Chat";
import { BsPlayCircle, BsDoorOpen } from "react-icons/bs";
import { PlayerScore, updatedPlayerScore } from "../../redux/slices/player-score-slice";
import { useContext } from "react";
import {
  ConnectionHubContext,
  LongRunningConnectionHubContext,
} from "../../context/ConnectionHubContext";
import HubEvents from "../../hub/HubMessages";
import Canvas from "../game-view-components/Canvas";
import { useDispatch } from "react-redux";
import { GameSettings, updatedGameSettings } from "../../redux/slices/game-settings-slice";
import loading from "./../../assets/loading.gif";
import {
  GameState,
  clearedGameState,
  updatedGameState,
  updatedIsGameStarted,
} from "../../redux/slices/game-state-slice";
import UrlHelper from "../../utils/UrlHelper";
import ClipboardBar from "../bars/ClipboardBar";
import ControlPanel from "../game-view-components/ControlPanel";
import { Player } from "../../interfaces/Player";
import { AnnouncementMessage } from "../../interfaces/AnnouncementMessage";
import { toast } from "react-toastify";
import { ToastNotificationEnum } from "../../enums/ToastNotificationEnum";
import "react-toastify/dist/ReactToastify.css";
import { SessionStorageService } from "../../classes/SessionStorageService";

function GameView() {
  const hub = useContext(ConnectionHubContext);
  const longRunningHub = useContext(LongRunningConnectionHubContext);

  const navigate = useNavigate();
  const dispatch = useDispatch();

  const player = useAppSelector((state) => state.player);
  const gameState = useAppSelector((state) => state.gameState);
  const gameSettings = useAppSelector((state) => state.gameSettings);

  const location = useLocation();
  const { fromViewNavigation } = location.state ?? false;

  const [playerScores, setPlayerScores] = useState<PlayerScore[]>([]);
  const [gameHash, setGameHash] = useState<string>("");
  const [isPlayerHost, setIsPlayerHost] = useState<boolean>(false);
  const [isGameDisplayed, setIsGameDisplayed] = useState<boolean>(false);
  const [isGameFinished, setIsGameFinished] = useState<boolean>(false);
  const [isStartGameButtonActive, setIsStartGameButtonActive] = useState<boolean>(false);

  const sessionStorageService = SessionStorageService.getInstance();

  const handleStartGameButtonClick = async () => {
    await longRunningHub.start();
    await longRunningHub.send(
      HubEvents.startGame,
      gameHash,
      sessionStorageService.getAuthorizationToken(),
      gameSettings
    );
  };

  const handleLeaveGameButtonClick = async () => {
    await hub.send(HubEvents.leaveGame, gameHash, sessionStorageService.getAuthorizationToken());
    sessionStorageService.clearAuthorizationToken();
    navigate(config.mainClientEndpoint);
  };

  useEffect(() => {
    setGameHash(UrlHelper.getGameHash(window.location.href));
  }, []);

  useEffect(() => {
    if (!gameHash) {
      return;
    }

    if (!fromViewNavigation) {
      navigate(`${config.joinGameClientEndpoint}/${gameHash}`);
      return;
    }

    const startHubAndJoinGame = async () => {
      const username = sessionStorageService.getUsername();

      hub.on(HubEvents.onUpdatePlayerScores, (playerScoresSerialized: string) => {
        const playerScores = JSON.parse(playerScoresSerialized) as PlayerScore[];
        setPlayerScores(playerScores);
      });

      hub.on(HubEvents.onStartGame, () => {
        dispatch(updatedIsGameStarted(true));
      });

      hub.on(
        HubEvents.onJoinGame,
        (playerSerialized: string, gameSettingsSerialized: string, gameStateSerialized: string) => {
          const player = JSON.parse(playerSerialized) as Player;
          const settings = JSON.parse(gameSettingsSerialized) as GameSettings;
          const state = JSON.parse(gameStateSerialized) as GameState;

          dispatch(updatedPlayerScore({ username: player.username, score: player.score }));
          dispatch(updatedGameSettings(settings));
          dispatch(updatedGameState(state));
          sessionStorageService.setAuthorizationToken(player.token);

          if (username == state.hostPlayerUsername) {
            setIsPlayerHost(true);
          }

          setTimeout(() => {
            setIsGameDisplayed(true);
          }, 1000);
        }
      );

      hub.on(HubEvents.onJoinGameError, (errorMessage: string) => {
        toast.error(errorMessage, { containerId: ToastNotificationEnum.Main });
        navigate(`${config.joinGameClientEndpoint}/${gameHash}`);
      });

      hub.on(HubEvents.onGameProblem, (message: string) => {
        const problemMessage = JSON.parse(message) as AnnouncementMessage;

        toast.warning(problemMessage.text, { containerId: ToastNotificationEnum.Main });
        navigate(config.mainClientEndpoint);
      });

      hub.on(HubEvents.onEndGame, () => {
        setIsGameFinished(true);
        toast.success("Game has been finished", { containerId: ToastNotificationEnum.Main });
        navigate(config.mainClientEndpoint);
      });

      const regex: RegExp = /^Player \d{4}$/;
      const playerUsername = regex ? username : player.username;

      await hub.start();
      await hub.invoke(
        HubEvents.joinGame,
        gameHash,
        sessionStorageService.getAuthorizationToken(),
        playerUsername
      );
    };

    const clearBeforeUnload = async () => {
      hub.off(HubEvents.onUpdatePlayerScores);
      hub.off(HubEvents.onStartGame);
      hub.off(HubEvents.onJoinGame);
      hub.off(HubEvents.onJoinGameError);
      hub.off(HubEvents.onGameProblem);

      if (!isGameFinished) {
        await hub.send(
          HubEvents.leaveGame,
          gameHash,
          sessionStorageService.getAuthorizationToken()
        );
      }

      dispatch(clearedGameState());
    };

    startHubAndJoinGame();

    window.addEventListener("beforeunload", clearBeforeUnload);
    window.addEventListener("unload", clearBeforeUnload);

    return () => {
      clearBeforeUnload();
      window.removeEventListener("beforeunload", clearBeforeUnload);
      window.removeEventListener("unload", clearBeforeUnload);
    };
  }, [gameHash]);

  useEffect(() => {
    if (playerScores.length > 1) {
      setIsStartGameButtonActive(true);
    } else {
      setIsStartGameButtonActive(false);
    }
  }, [playerScores]);

  return (
    <>
      {!isGameDisplayed ? (
        <div className="d-flex justify-content-center align-items-center mt-4">
          <img src={loading} alt="Loading" className="w-30 h-30 img-fluid" />
        </div>
      ) : (
        <>
          {gameState.isGameStarted ? (
            <div className="container text-center">
              <div className="row">
                <div className="col-xl-2 col-lg-6 col-md-6 col-12 order-xl-1 order-lg-2 order-md-2 order-3 mb-3">
                  <PlayerScores
                    title={"Scoreboard"}
                    playerScores={playerScores}
                    displayPoints={true}
                    displayIndex={true}
                    displayRound={true}
                  />
                  <ControlPanel onClick={handleLeaveGameButtonClick} />
                </div>
                <div className="col-xl-7 col-lg-12 col-md-12 col-12 order-xl-2 order-lg-1 order-md-1 order-1 mb-3">
                  <Canvas />
                </div>
                <div className="col-xl-3 col-lg-6 col-md-6 col-12 order-xl-2 order-lg-3 order-md-3 order-2 mb-3">
                  <Chat placeholderValue="Enter your guess" displaySecretWord={true} />
                </div>
              </div>
            </div>
          ) : (
            <div className="container mb-3">
              <div className="row">
                <div className="col-lg-4 col-sm-5 col-12 mx-auto mt-2 text-center order-lg-1 order-2 mb-3">
                  <div className="col-lg-6">
                    <PlayerScores
                      title={"Players in the lobby"}
                      playerScores={playerScores}
                      displayPoints={false}
                      displayIndex={false}
                      displayRound={false}
                    />
                    <ClipboardBar invitationUrl={window.location.href} />
                  </div>
                </div>
                <div className="col-lg-4 col-sm-10 col-12 mx-auto text-center order-lg-2 order-1">
                  <h5>Your username: {player.username}</h5>
                  {isPlayerHost && (
                    <Button
                      text="Start the game"
                      type="success"
                      active={isStartGameButtonActive}
                      icon={<BsPlayCircle />}
                      onClick={handleStartGameButtonClick}
                    />
                  )}
                  {!isPlayerHost && (
                    <h4 className="mt-3">Waiting for the host to start the game</h4>
                  )}
                  <Button
                    text="Leave the game"
                    active={true}
                    icon={<BsDoorOpen />}
                    type={"danger"}
                    onClick={handleLeaveGameButtonClick}
                  />
                  <div className="mt-3">
                    <GameSettingsBoard isPlayerHost={isPlayerHost} />
                  </div>
                </div>
                <div className="col-lg-4 order-lg-3 col-sm-7 col-12 order-3">
                  <div className="col-lg-9 col-sm-12 col-12 float-end mb-3">
                    <Chat placeholderValue={"Enter your message"} displaySecretWord={false} />
                  </div>
                </div>
              </div>
            </div>
          )}
        </>
      )}
    </>
  );
}

export default GameView;
