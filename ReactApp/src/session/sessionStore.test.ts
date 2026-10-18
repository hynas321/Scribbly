import { describe, expect, it, vi } from "vitest";
import { createSessionStore } from "./sessionStore";

const setup = () => {
  const data = new Map<string, string>();
  return createSessionStore({
    getItem: (key) => data.get(key) ?? null,
    setItem: (key, value) => {
      data.set(key, value);
    },
    removeItem: (key) => {
      data.delete(key);
    },
  });
};

describe("tab-scoped room session store", () => {
  it("stores one room credential and active room hash", () => {
    const store = setup();
    store.set("roomAccessToken", "opaque-room-token");
    store.set("activeGameHash", "ROOM123");
    expect(store.get("roomAccessToken")).toBe("opaque-room-token");
    expect(store.get("activeGameHash")).toBe("ROOM123");
  });

  it("notifies active subscribers exactly once on removal", () => {
    const store = setup();
    store.set("roomAccessToken", "token");
    const listener = vi.fn();
    store.subscribe(listener);
    store.remove("roomAccessToken");
    store.remove("roomAccessToken");
    expect(listener).toHaveBeenCalledTimes(1);
  });
});
