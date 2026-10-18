type SessionKey =
  | "username"
  | "roomAccessToken"
  | "activeGameHash"
  | "completedGameHash"
  | "completedRoomAccessToken"
  | "resultGameHash"
  | "resultRoomAccessToken";

export function createSessionStore(storage: Pick<Storage, "getItem" | "setItem" | "removeItem">) {
  const listeners = new Set<() => void>();
  const notify = () => listeners.forEach((listener) => listener());
  return {
    get: (key: SessionKey) => storage.getItem(key) ?? "",
    set(key: SessionKey, value: string) {
      if (storage.getItem(key) === value) {
        return;
      }
      storage.setItem(key, value);
      notify();
    },
    remove(key: SessionKey) {
      if (storage.getItem(key) === null) {
        return;
      }
      storage.removeItem(key);
      notify();
    },
    subscribe(listener: () => void) {
      listeners.add(listener);
      return () => {
        listeners.delete(listener);
      };
    },
  };
}

export const sessionStore = createSessionStore({
  getItem: (key) => sessionStorage.getItem(key),
  setItem: (key, value) => sessionStorage.setItem(key, value),
  removeItem: (key) => sessionStorage.removeItem(key),
});
