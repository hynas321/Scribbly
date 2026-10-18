import * as signalR from "@microsoft/signalr";
import { describe, expect, it, vi } from "vitest";
import Hub from "./Hub";

const createConnection = (state = signalR.HubConnectionState.Disconnected) => ({
  state,
  start: vi.fn<[], Promise<void>>(async () => undefined),
  stop: vi.fn<[], Promise<void>>(async () => undefined),
  invoke: vi.fn<[string, ...unknown[]], Promise<unknown>>(async () => undefined),
  on: vi.fn(),
  off: vi.fn(),
  onreconnected: vi.fn(),
  onreconnecting: vi.fn(),
  onclose: vi.fn(),
});

describe("Hub lifecycle", () => {
  it("coalesces concurrent starts", async () => {
    const connection = createConnection();
    let release!: () => void;
    connection.start.mockImplementation(
      () =>
        new Promise<void>((resolve) => {
          release = resolve;
        }),
    );
    const hub = new Hub("/test", connection as unknown as signalR.HubConnection);
    const first = hub.start();
    const second = hub.start();
    expect(connection.start).toHaveBeenCalledTimes(1);
    release();
    await Promise.all([first, second]);
  });

  it("rejects invoke while disconnected and forwards it while connected", async () => {
    const connection = createConnection();
    const hub = new Hub("/test", connection as unknown as signalR.HubConnection);
    await expect(hub.invoke("JoinGame", "ABC")).rejects.toThrow("Disconnected");
    connection.state = signalR.HubConnectionState.Connected;
    connection.invoke.mockResolvedValue({ username: "alice" });
    await expect(hub.invoke("JoinGame", "ABC")).resolves.toEqual({ username: "alice" });
  });

  it("publishes Connected only after reconnect restoration callbacks finish", async () => {
    const connection = createConnection();
    const hub = new Hub("/test", connection as unknown as signalR.HubConnection);
    const order: string[] = [];
    hub.subscribeState(() => order.push("state"));
    hub.onReconnected(async () => {
      order.push("restore-start");
      await Promise.resolve();
      order.push("restore-end");
    });
    connection.state = signalR.HubConnectionState.Connected;
    await connection.onreconnected.mock.calls[0][0]("new-id");
    expect(order.slice(-3)).toEqual(["restore-start", "restore-end", "state"]);
  });
});
