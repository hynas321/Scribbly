import { useCallback, useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import config from "../../../config.json";
import Hub from "../../hub/Hub";
import HubEvents from "../../hub/HubMessages";
import { RoomSnapshot, PromptView } from "../../hub/HubInterfaces";
import { PlayerScore } from "../../interfaces/PlayerScore";
import { AnnouncementMessage } from "../../interfaces/AnnouncementMessage";
import { ToastNotificationEnum } from "../../enums/ToastNotificationEnum";
import { useAppDispatch } from "../../redux/hooks";
import {
  clearedGameState,
  updatedCorrectGuessPlayerUsernames,
  updatedCurrentDrawingTimeSeconds,
  updatedCurrentRound,
  updatedDrawingPlayerUsername,
  updatedGameState,
  updatedHostPlayerUsername,
  updatedIsGameStarted,
  updatedIsTimerVisible,
  updatedPrompt,
} from "../../redux/slices/game-state-slice";
import { updatedGameSettings } from "../../redux/slices/game-settings-slice";
import { updatedPlayerScore } from "../../redux/slices/player-score-slice";
import { useSessionStorage } from "../../hooks/useSessionStorage";
import { translate, translateAnnouncement } from "../../i18n/I18nContext";
import api from "../../http/Api";

export interface RoomSessionState {
  playerScores: PlayerScore[];
  isPlayerHost: boolean;
  isGameDisplayed: boolean;
}

export const useRoomSession = (hub: Hub, gameHash: string): RoomSessionState => {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const {
    username,
    roomAccessToken,
    activeGameHash,
    resultGameHash,
    resultRoomAccessToken,
    setUsername,
    setActiveGameHash,
    markCompletedGame,
    clearGameSession,
  } = useSessionStorage();
  const tokenRef = useRef(roomAccessToken);
  tokenRef.current = roomAccessToken;
  const usernameRef = useRef(username);
  usernameRef.current = username;
  const [playerScores, setPlayerScores] = useState<PlayerScore[]>([]);
  const [isPlayerHost, setIsPlayerHost] = useState(false);
  const [isGameDisplayed, setIsGameDisplayed] = useState(false);

  const applySnapshot = useCallback(
    (snapshot: RoomSnapshot) => {
      setUsername(snapshot.username);
      setActiveGameHash(snapshot.gameHash || gameHash);
      setPlayerScores(snapshot.scores);
      setIsPlayerHost(snapshot.isHost);
      dispatch(updatedGameSettings(snapshot.settings));
      dispatch(updatedGameState(snapshot.state));
      dispatch(
        updatedPlayerScore(
          snapshot.scores.find((score) => score.username === snapshot.username) ?? {
            username: snapshot.username,
            score: 0,
          },
        ),
      );
      setIsGameDisplayed(true);
    },
    [dispatch, gameHash, setActiveGameHash, setUsername],
  );

  useEffect(() => {
    if (resultGameHash === gameHash && resultRoomAccessToken && !roomAccessToken) {
      navigate(`${config.gameClientEndpoint}/${gameHash}/results`, { replace: true });
      return;
    }
    if (!gameHash || !roomAccessToken || (activeGameHash && activeGameHash !== gameHash)) {
      navigate(`${config.joinGameClientEndpoint}/${gameHash}`, { replace: true });
      return;
    }
    let active = true;
    const disposers: Array<() => void> = [];

    const routeToJoin = (message?: string) => {
      if (!active) {
        return;
      }
      if (message) {
        toast.error(message, { containerId: ToastNotificationEnum.Main });
      }
      clearGameSession();
      navigate(`${config.joinGameClientEndpoint}/${gameHash}`, { replace: true });
    };

    const refreshPrompt = async () => {
      try {
        const prompt = await hub.invoke<PromptView>(HubEvents.getPrompt, gameHash);
        if (active) {
          dispatch(
            updatedPrompt({ text: prompt.text, category: prompt.category, kind: prompt.kind }),
          );
        }
      } catch {}
    };

    const restoreOrJoin = async () => {
      const token = tokenRef.current;
      if (!token) {
        return routeToJoin();
      }
      try {
        const completed = await api.getCompletedGameResult(gameHash, token);
        if (!active) {
          return;
        }
        if (completed.completed) {
          markCompletedGame(gameHash, token);
          dispatch(updatedIsGameStarted(false));
          navigate(`${config.gameClientEndpoint}/${gameHash}/results`, { replace: true });
          return;
        }
        const resumed = await hub.invoke<RoomSnapshot | null>(HubEvents.reconnectGame, gameHash);
        if (!active) {
          return;
        }
        if (resumed) {
          applySnapshot(resumed);
          if (resumed.state.isGameStarted) {
            await refreshPrompt();
          }
          return;
        }

        const exists = await api.checkIfGameExists(gameHash);
        if (!active) {
          return;
        }
        if (exists) {
          const joined = await hub.invoke<RoomSnapshot>(HubEvents.joinGame, gameHash);
          if (active) {
            applySnapshot(joined);
          }
          return;
        }

        routeToJoin(translate("session.noLongerExists"));
      } catch (error) {
        routeToJoin(error instanceof Error ? error.message : translate("session.restoreFailed"));
      }
    };

    disposers.push(
      hub.on(HubEvents.onPlayerScoresUpdated, (scores: PlayerScore[]) => {
        if (active) {
          setPlayerScores(scores);
        }
      }),
      hub.on(HubEvents.onGameStarted, () => {
        if (active) {
          dispatch(updatedIsGameStarted(true));
        }
      }),
      hub.on(HubEvents.onTimerUpdated, (seconds: number) => {
        if (active) {
          dispatch(updatedCurrentDrawingTimeSeconds(seconds));
        }
      }),
      hub.on(HubEvents.onTimerVisibilityUpdated, (visible: boolean) => {
        if (active) {
          dispatch(updatedIsTimerVisible(visible));
        }
      }),
      hub.on(HubEvents.onDrawingPlayerUpdated, (username: string) => {
        if (active) {
          dispatch(updatedDrawingPlayerUsername(username));
        }
      }),
      hub.on(HubEvents.onCurrentRoundUpdated, (round: number) => {
        if (active) {
          dispatch(updatedCurrentRound(round));
        }
      }),
      hub.on(HubEvents.onCorrectGuessPlayersUpdated, (usernames: string[]) => {
        if (active) {
          dispatch(updatedCorrectGuessPlayerUsernames(usernames));
        }
      }),
      hub.on(HubEvents.onPromptRefreshRequested, () => {
        void refreshPrompt();
      }),
      hub.on(HubEvents.onHostChanged, (hostUsername: string) => {
        if (!active) {
          return;
        }
        dispatch(updatedHostPlayerUsername(hostUsername));
        setIsPlayerHost(hostUsername === usernameRef.current);
      }),
      hub.on(HubEvents.onPlayerKicked, (kickedUsername: string) => {
        if (!active) {
          return;
        }
        if (kickedUsername === usernameRef.current) {
          toast.error(translate("kick.youWereKicked"), { containerId: ToastNotificationEnum.Main });
          clearGameSession();
          navigate(config.mainClientEndpoint, { replace: true });
        }
      }),
      hub.on(HubEvents.onGameProblem, (message: AnnouncementMessage) => {
        if (!active) {
          return;
        }
        const localized = translateAnnouncement(message.text);
        toast.warning(localized, { containerId: ToastNotificationEnum.Main });
        clearGameSession();
        navigate(config.mainClientEndpoint, { replace: true });
      }),
      hub.on(HubEvents.onGameSessionFailed, (message: string) => {
        if (!active) {
          return;
        }
        dispatch(updatedIsGameStarted(false));
        toast.error(message || translate("session.stopped"), {
          containerId: ToastNotificationEnum.Main,
        });
      }),
      hub.on(HubEvents.onGameEnded, () => {
        if (!active) {
          return;
        }
        const token = tokenRef.current;
        if (token) {
          markCompletedGame(gameHash, token);
        } else {
          clearGameSession();
        }
        dispatch(updatedIsGameStarted(false));
        navigate(`${config.gameClientEndpoint}/${gameHash}/results`, { replace: true });
      }),
      hub.onReconnected(restoreOrJoin),
    );

    void hub
      .start()
      .then(restoreOrJoin)
      .catch(() => routeToJoin(translate("session.connectFailed")));

    return () => {
      active = false;
      disposers.forEach((dispose) => dispose());
      dispatch(clearedGameState());
      void hub.stop().catch(() => undefined);
    };
  }, [
    gameHash,
    roomAccessToken,
    activeGameHash,
    hub,
    navigate,
    dispatch,
    applySnapshot,
    clearGameSession,
    markCompletedGame,
    resultGameHash,
    resultRoomAccessToken,
  ]);

  return { playerScores, isPlayerHost, isGameDisplayed };
};
