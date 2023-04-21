import * as signalR from "@microsoft/signalr";
import config from "../../../config.json"

class Hub {
    private connection: signalR.HubConnection

    constructor() {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(`${config.httpServerUrl}${config.hubLobbyEndpoint}`, {
          skipNegotiation: true,
          transport: signalR.HttpTransportType.WebSockets,
          withCredentials: false,
        })
        .build();
    }

    async start() {
      if (this.connection.state === signalR.HubConnectionState.Disconnected) {
        return await this.connection.start();
      }
    }

    async stop() {
      if (this.connection.state === signalR.HubConnectionState.Connected) {
        return await this.connection.stop();
      }
    }

    async invoke(name: string, data: any) {
      if (this.connection.state === signalR.HubConnectionState.Connected) {
        return await this.connection.invoke(name, data);
      }
    }

    on(name: string, callback: (...args: any[]) => any) {
      this.connection.on(name, callback);
    }
}

export default Hub;