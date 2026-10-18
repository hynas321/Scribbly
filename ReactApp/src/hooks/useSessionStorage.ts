import { useSyncExternalStore } from "react";
import { sessionStore } from "../session/sessionStore";

const setUsername = (value: string) => sessionStore.set("username", value);
const setRoomAccessToken = (value: string) => sessionStore.set("roomAccessToken", value);
const clearRoomAccessToken = () => sessionStore.remove("roomAccessToken");
const setActiveGameHash = (value: string) => sessionStore.set("activeGameHash", value);
const clearActiveGameHash = () => sessionStore.remove("activeGameHash");
const markCompletedGame = (gameHash: string, roomAccessToken: string) => {
  if (gameHash && roomAccessToken) {
    sessionStore.set("resultGameHash", gameHash);
    sessionStore.set("resultRoomAccessToken", roomAccessToken);
    sessionStore.set("completedGameHash", gameHash);
    sessionStore.set("completedRoomAccessToken", roomAccessToken);
  }
  clearRoomAccessToken();
  clearActiveGameHash();
};
const clearCompletedGame = () => {
  sessionStore.remove("completedGameHash");
  sessionStore.remove("completedRoomAccessToken");
};
const clearGameSession = () => {
  clearRoomAccessToken();
  clearActiveGameHash();
};

export const useSessionStorage = () => ({
  resultGameHash: useSyncExternalStore(sessionStore.subscribe, () =>
    sessionStore.get("resultGameHash"),
  ),
  resultRoomAccessToken: useSyncExternalStore(sessionStore.subscribe, () =>
    sessionStore.get("resultRoomAccessToken"),
  ),
  username: useSyncExternalStore(sessionStore.subscribe, () => sessionStore.get("username")),
  roomAccessToken: useSyncExternalStore(sessionStore.subscribe, () =>
    sessionStore.get("roomAccessToken"),
  ),
  activeGameHash: useSyncExternalStore(sessionStore.subscribe, () =>
    sessionStore.get("activeGameHash"),
  ),
  completedGameHash: useSyncExternalStore(sessionStore.subscribe, () =>
    sessionStore.get("completedGameHash"),
  ),
  completedRoomAccessToken: useSyncExternalStore(sessionStore.subscribe, () =>
    sessionStore.get("completedRoomAccessToken"),
  ),
  setUsername,
  setRoomAccessToken,
  clearRoomAccessToken,
  setActiveGameHash,
  clearActiveGameHash,
  markCompletedGame,
  clearCompletedGame,
  clearGameSession,
});
