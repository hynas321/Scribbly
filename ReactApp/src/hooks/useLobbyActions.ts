import { useRef } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import { ToastNotificationEnum } from "../enums/ToastNotificationEnum";
import config from "../../config.json";
import HubEvents from "../hub/HubMessages";
import Hub from "../hub/Hub";
import { useSessionStorage } from "./useSessionStorage";
import { GameSettings } from "../redux/slices/game-settings-slice";
import { useTranslation } from "../i18n/I18nContext";

export const useLobbyActions = (hub: Hub, gameHash: string, gameSettings: GameSettings) => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { roomAccessToken, clearGameSession } = useSessionStorage();
  const startPending = useRef(false);
  const leavePending = useRef(false);

  const handleStartGame = async () => {
    if (!roomAccessToken || startPending.current) {
      return;
    }
    startPending.current = true;
    try {
      await hub.invoke<string>(HubEvents.startGame, gameHash, gameSettings);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : t("error.startGame"), {
        containerId: ToastNotificationEnum.Main,
      });
    } finally {
      startPending.current = false;
    }
  };

  const handleLeaveGame = async () => {
    if (leavePending.current) {
      return;
    }
    leavePending.current = true;
    try {
      if (roomAccessToken) {
        await hub.invoke(HubEvents.leaveGame, gameHash);
      }
    } catch {
    } finally {
      clearGameSession();
      await hub.stop().catch(() => undefined);
      navigate(config.mainClientEndpoint, { replace: true });
      leavePending.current = false;
    }
  };

  return { handleStartGame, handleLeaveGame };
};
