import { useNavigate } from "react-router-dom";
import config from "./../../config.json";
import HubEvents from "../hub/HubMessages";
import Hub from "./../hub/Hub";
import { useSessionStorage } from "./useSessionStorage";
import { GameSettings } from "../redux/slices/game-settings-slice";

export const useLobbyActions = (hub: Hub, longRunningHub: Hub, gameHash: string, gameSettings: GameSettings) => {
  const navigate = useNavigate();
  const { authorizationToken, clearAuthorizationToken } = useSessionStorage();

  const handleStartGame = async () => {
    try {
      await longRunningHub.start();
      await longRunningHub.send(HubEvents.startGame, gameHash, authorizationToken, gameSettings);
    } catch (err) {
      console.error("Failed to start game:", err);
    }
  };

  const handleLeaveGame = async () => {
    try {
      await hub.send(HubEvents.leaveGame, gameHash, authorizationToken);
      clearAuthorizationToken();
      navigate(config.mainClientEndpoint);
      await hub.stop();
    } catch (err) {
      console.error("Failed to leave game:", err);
    }
  };

  return { handleStartGame, handleLeaveGame };
};
