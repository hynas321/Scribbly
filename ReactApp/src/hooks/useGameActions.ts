import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import { ToastNotificationEnum } from "../enums/ToastNotificationEnum";
import config from "../../config.json";
import { useSessionStorage } from "./useSessionStorage";
import { useTranslation } from "../i18n/I18nContext";
import api from "../http/Api";

export const useGameActions = (username: string) => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { setUsername, setRoomAccessToken, setActiveGameHash, clearGameSession } =
    useSessionStorage();

  const enterRoom = (gameHash: string, token: string, normalizedUsername: string) => {
    clearGameSession();
    setUsername(normalizedUsername);
    setRoomAccessToken(token);
    setActiveGameHash(gameHash);
    navigate(`${config.gameClientEndpoint}/${gameHash}`);
  };

  const createGame = async () => {
    const normalized = username.trim();
    if (!normalized) {
      return;
    }
    try {
      const result = await api.createGame(normalized);
      enterRoom(result.gameHash, result.roomAccessToken, normalized);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : t("error.createGame"), {
        containerId: ToastNotificationEnum.Main,
      });
    }
  };

  const joinGame = async (gameHash: string) => {
    const normalizedHash = gameHash.trim();
    const normalizedUsername = username.trim();
    if (!normalizedHash || !normalizedUsername) {
      return;
    }
    try {
      const credential = await api.issueJoinCredential(normalizedHash, normalizedUsername);
      enterRoom(normalizedHash, credential.roomAccessToken, normalizedUsername);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : t("error.joinGame"), {
        containerId: ToastNotificationEnum.Main,
      });
    }
  };

  return { createGame, joinGame };
};
