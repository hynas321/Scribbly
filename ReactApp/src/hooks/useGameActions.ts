import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import { useAppDispatch } from "../redux/hooks";
import { ToastNotificationEnum } from "../enums/ToastNotificationEnum";
import { updatedUsername } from "../redux/slices/player-score-slice";
import config from "../../config.json";
import { useSessionStorage } from "./useSessionStorage";
import api from "../http/Api";

export const useGameActions = (username: string) => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { setAuthorizationToken } = useSessionStorage();

  const createGame = async () => {
    try {
      const result = await api.createGame(username);
      if (!("hostToken" in result) || !result.gameHash) {
        toast.error("Could not create the game, try again", { containerId: ToastNotificationEnum.Main });
        return;
      }
      setAuthorizationToken(result.hostToken);
      dispatch(updatedUsername(username));
      navigate(`${config.gameClientEndpoint}/${result.gameHash}`, { state: { fromViewNavigation: true } });
    } catch {
      toast.error("Unexpected error", { containerId: ToastNotificationEnum.Main });
    }
  };

  const joinGame = async (gameHash: string) => {
    try {
      const exists = await api.checkIfGameExists(gameHash);

      if (!exists) {
        toast.error("Game does not exist", { containerId: ToastNotificationEnum.Main });
        return;
      }
      dispatch(updatedUsername(username));
      navigate(`${config.gameClientEndpoint}/${gameHash}`, { state: { fromViewNavigation: true } });
    } catch {
      toast.error("Unexpected error", { containerId: ToastNotificationEnum.Main });
    }
  };

  return { createGame, joinGame };
};
