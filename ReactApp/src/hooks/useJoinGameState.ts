import { useMemo } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { toast } from "react-toastify";
import { ToastNotificationEnum } from "../enums/ToastNotificationEnum";
import config from "../../config.json";
import { useSessionStorage } from "./useSessionStorage";
import { useTranslation } from "../i18n/I18nContext";
import api from "../http/Api";

export const useJoinGameState = () => {
  const { t } = useTranslation();
  const minUsernameLength = 1;
  const maxUsernameLength = 25;
  const navigate = useNavigate();
  const { gameHash = "" } = useParams();
  const { username, setUsername, setRoomAccessToken, setActiveGameHash, clearGameSession } =
    useSessionStorage();
  const normalizedUsername = username.trim();
  const isJoinGameButtonActive = useMemo(
    () =>
      normalizedUsername.length >= minUsernameLength &&
      normalizedUsername.length <= maxUsernameLength,
    [normalizedUsername],
  );

  const handleJoinGame = async () => {
    if (!isJoinGameButtonActive || !gameHash) {
      return;
    }
    try {
      const credential = await api.issueJoinCredential(gameHash, normalizedUsername);
      clearGameSession();
      setUsername(normalizedUsername);
      setRoomAccessToken(credential.roomAccessToken);
      setActiveGameHash(gameHash);
      navigate(`${config.gameClientEndpoint}/${gameHash}`, { replace: true });
    } catch (error) {
      toast.error(error instanceof Error ? error.message : t("error.joinGame"), {
        containerId: ToastNotificationEnum.Main,
      });
    }
  };

  return {
    username,
    setUsername,
    isJoinGameButtonActive,
    handleJoinGame,
    minUsernameLength,
    maxUsernameLength,
  };
};
