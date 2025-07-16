import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import config from "../../../config.json";
import HubEvents from "../../hub/HubMessages";
import { PlayerScore } from "../../interfaces/PlayerScore";
import { clearedGameState, GameState, updatedGameState, updatedIsGameStarted } from "../../redux/slices/game-state-slice";
import { Player } from "../../interfaces/Player";
import { GameSettings, updatedGameSettings } from "../../redux/slices/game-settings-slice";
import { updatedPlayerScore } from "../../redux/slices/player-score-slice";
import { useSessionStorage } from "../useSessionStorage";
import { ToastNotificationEnum } from "../../enums/ToastNotificationEnum";
import { AnnouncementMessage } from "../../interfaces/AnnouncementMessage";
import { toast } from "react-toastify";
import { useDispatch } from "react-redux";
import Hub from "../../hub/Hub";

export const useGameHubEvents = (
  hub: Hub,
  gameHash: string,
  setPlayerScores: any,
  setIsPlayerHost: any,
  setIsGameDisplayed: any,
  fromViewNavigation: any,
  isGameFinished: boolean,
  setIsGameFinished: any
) => {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { authorizationToken, setAuthorizationToken, username } = useSessionStorage();

  useEffect(() => {
    if (!gameHash) return;

    if (!fromViewNavigation) {
      navigate(`${config.joinGameClientEndpoint}/${gameHash}`);
      return;
    }

    const startHubAndJoinGame = async () => {
      hub.on(HubEvents.onUpdatePlayerScores, (playerScoresSerialized: string) => {
        const playerScores = JSON.parse(playerScoresSerialized) as PlayerScore[];
        setPlayerScores(playerScores);
      });

      hub.on(HubEvents.onStartGame, () => {
        dispatch(updatedIsGameStarted(true));
      });

      hub.on(HubEvents.onJoinGame,
        (playerSerialized: string, gameSettingsSerialized: string, gameStateSerialized: string) => {
          const player = JSON.parse(playerSerialized) as Player;
          const settings = JSON.parse(gameSettingsSerialized) as GameSettings;
          const state = JSON.parse(gameStateSerialized) as GameState;

          dispatch(updatedPlayerScore({ username: player.username, score: player.score }));
          dispatch(updatedGameSettings(settings));
          dispatch(updatedGameState(state));
          setAuthorizationToken(player.token);

          if (username === state.hostPlayerUsername) {
            setIsPlayerHost(true);
          }

          setTimeout(() => setIsGameDisplayed(true), 1000);
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

      await hub.start();
      await hub.invoke(HubEvents.joinGame, gameHash, authorizationToken, username);
    };

    const clearBeforeUnload = async () => {
      hub.off(HubEvents.onUpdatePlayerScores);
      hub.off(HubEvents.onStartGame);
      hub.off(HubEvents.onJoinGame);
      hub.off(HubEvents.onJoinGameError);
      hub.off(HubEvents.onGameProblem);

      if (!isGameFinished) {
        await hub.send(HubEvents.leaveGame, gameHash, authorizationToken);
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
};
