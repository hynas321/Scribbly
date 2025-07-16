import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import HttpRequestHandler from "../http/Api";
import { useDispatch } from "react-redux";
import { useSessionStorage } from "./useSessionStorage";
import { MainScoreboardScore } from "../interfaces/MainScoreboardScore";
import UrlHelper from "../utils/UrlHelper";
import { toast } from "react-toastify";
import { ToastNotificationEnum } from "../enums/ToastNotificationEnum";
import config from "../../config.json";
import { updatedUsername } from "../redux/slices/player-score-slice";
import api from "../http/Api";

export const useJoinGameState = () => {
  const minUsernameLength = 1;
  const maxUsernameLength = 18;

  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { username, setUsername } = useSessionStorage();

  const [gameHash, setGameHash] = useState("");
  const [isJoinGameButtonActive, setIsJoinGameButtonActive] = useState(false);

  useEffect(() => {
    setGameHash(UrlHelper.getGameHash(window.location.href));
  }, []);

  useEffect(() => {
    setIsJoinGameButtonActive(
      username.length >= minUsernameLength && username.length <= maxUsernameLength
    );
  }, [username]);

  const handleJoinGame = async () => {
    if (username.length < minUsernameLength) return;

    try {
      const gameExists = await api.checkIfGameExists(gameHash);
      if (gameExists) {
        dispatch(updatedUsername(username));
        navigate(`${config.gameClientEndpoint}/${gameHash}`, {
          state: { fromViewNavigation: true },
        });
      } else {
        toast.error("Game does not exist", { containerId: ToastNotificationEnum.Main });
        navigate(`${config.mainClientEndpoint}`);
      }
    } catch {
      toast.error("Unexpected error", { containerId: ToastNotificationEnum.Main });
      navigate(`${config.mainClientEndpoint}`);
    }
  };

  return {
    username,
    setUsername,
    isJoinGameButtonActive,
    handleJoinGame,
    minUsernameLength,
    maxUsernameLength
  };
};