import * as signalR from "@microsoft/signalr";

type AccessTokenProvider = () => string;

const retryPolicy: signalR.IRetryPolicy = {
  nextRetryDelayInMilliseconds(context) {
    if (context.elapsedMilliseconds >= 110_000) {
      return null;
    }
    const delays = [0, 1_000, 2_000, 5_000, 10_000, 20_000, 30_000];
    const base = delays[Math.min(context.previousRetryCount, delays.length - 1)];
    return base === 0 ? 0 : base + Math.floor(Math.random() * Math.min(1_000, base * 0.2));
  },
};

class Hub {
  private readonly connection: signalR.HubConnection;
  private startPromise?: Promise<void>;
  private stopPromise?: Promise<void>;
  private restoringSession = false;
  private lifecycleBound = false;
  private readonly stateListeners = new Set<(state: signalR.HubConnectionState) => void>();
  private readonly reconnectedListeners = new Set<
    (connectionId?: string) => void | Promise<void>
  >();
  private readonly reconnectingListeners = new Set<(error?: Error) => void>();
  private readonly closedListeners = new Set<(error?: Error) => void>();

  constructor(
    hubPath: string,
    connection?: signalR.HubConnection,
    accessTokenProvider?: AccessTokenProvider,
  ) {
    if (connection) {
      this.connection = connection;
      return;
    }
    const configuredServer = (import.meta.env.VITE_SERVER_URL ?? "").trim();
    const baseUrl = (configuredServer || window.location.origin).replace(/\/$/, "");
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${baseUrl}${hubPath}`, {
        withCredentials: true,
        accessTokenFactory: () => accessTokenProvider?.() ?? "",
      })
      .withAutomaticReconnect(retryPolicy)
      .configureLogging(import.meta.env.DEV ? signalR.LogLevel.Warning : signalR.LogLevel.Error)
      .build();
    this.connection.serverTimeoutInMilliseconds = 35_000;
    this.connection.keepAliveIntervalInMilliseconds = 15_000;
  }

  async start(): Promise<void> {
    if (this.stopPromise) {
      await this.stopPromise;
    }
    if (this.startPromise) {
      return this.startPromise;
    }
    if (this.connection.state !== signalR.HubConnectionState.Disconnected) {
      return;
    }
    this.startPromise = this.startWithRetry().finally(() => {
      this.startPromise = undefined;
      this.notifyState();
    });
    this.notifyState();
    return this.startPromise;
  }

  async stop(): Promise<void> {
    if (this.stopPromise) {
      return this.stopPromise;
    }
    if (this.connection.state === signalR.HubConnectionState.Disconnected) {
      return;
    }
    this.stopPromise = this.connection.stop().finally(() => {
      this.stopPromise = undefined;
      this.notifyState();
    });
    return this.stopPromise;
  }

  async invoke<TResult = void>(name: string, ...args: unknown[]): Promise<TResult> {
    if (this.connection.state !== signalR.HubConnectionState.Connected) {
      throw new Error(`Cannot execute '${name}' while SignalR is ${this.connection.state}.`);
    }
    return this.connection.invoke<TResult>(name, ...args);
  }

  on<TArgs extends unknown[]>(name: string, callback: (...args: TArgs) => void): () => void {
    const handler = (...args: unknown[]) => callback(...(args as TArgs));
    this.connection.on(name, handler);
    return () => this.connection.off(name, handler);
  }

  onReconnected(callback: (connectionId?: string) => void | Promise<void>) {
    this.bindLifecycle();
    this.reconnectedListeners.add(callback);
    return () => {
      this.reconnectedListeners.delete(callback);
    };
  }
  onReconnecting(callback: (error?: Error) => void) {
    this.bindLifecycle();
    this.reconnectingListeners.add(callback);
    return () => {
      this.reconnectingListeners.delete(callback);
    };
  }
  onClosed(callback: (error?: Error) => void) {
    this.bindLifecycle();
    this.closedListeners.add(callback);
    return () => {
      this.closedListeners.delete(callback);
    };
  }
  subscribeState(listener: (state: signalR.HubConnectionState) => void) {
    this.bindLifecycle();
    this.stateListeners.add(listener);
    listener(this.getState());
    return () => {
      this.stateListeners.delete(listener);
    };
  }
  getConnection() {
    return this.connection;
  }
  getState() {
    const isRestoringConnectedSession =
      this.restoringSession && this.connection.state === signalR.HubConnectionState.Connected;

    return isRestoringConnectedSession
      ? signalR.HubConnectionState.Reconnecting
      : this.connection.state;
  }

  private async startWithRetry() {
    const delays = [0, 500, 1_500, 4_000];
    let lastError: unknown;
    for (const delay of delays) {
      if (delay > 0) {
        await new Promise<void>((resolve) => window.setTimeout(resolve, delay));
      }
      if (this.connection.state !== signalR.HubConnectionState.Disconnected) {
        return;
      }
      try {
        await this.connection.start();
        return;
      } catch (error) {
        lastError = error;
      }
    }
    throw lastError instanceof Error ? lastError : new Error("SignalR connection failed.");
  }

  private bindLifecycle() {
    if (this.lifecycleBound) {
      return;
    }
    this.lifecycleBound = true;
    this.connection.onreconnecting((error) => {
      this.reconnectingListeners.forEach((listener) => listener(error));
      this.notifyState();
    });
    this.connection.onreconnected(async (connectionId) => {
      this.restoringSession = true;
      await Promise.allSettled(
        [...this.reconnectedListeners].map((listener) => Promise.resolve(listener(connectionId))),
      );
      this.restoringSession = false;
      this.notifyState();
    });
    this.connection.onclose((error) => {
      this.restoringSession = false;
      this.closedListeners.forEach((listener) => listener(error));
      this.notifyState();
    });
  }

  private notifyState() {
    const state = this.getState();
    this.stateListeners.forEach((listener) => listener(state));
  }
}

export default Hub;
