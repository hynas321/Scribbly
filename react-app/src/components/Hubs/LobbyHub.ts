import * as signalR from "@microsoft/signalr";
import config from "../../../config.json"

class LobbyHub {
    private connection: signalR.HubConnection

    constructor(url: string) {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(`${config.httpServerUrl}${config.hubLobbyEndpoint}`, {
          skipNegotiation: true,
          transport: signalR.HttpTransportType.WebSockets,
          withCredentials: false,
        })
        .build();

      this.connection.on("PlayerJoinedLobby", () => {
        console.log("Player joined lobby")
      });
    }

    start() {
      if (this.connection.state === signalR.HubConnectionState.Disconnected) {
        this.connection.start();
      }
    }

    stop() {
      if (this.connection.state === signalR.HubConnectionState.Connected) {
        this.connection.stop();
      }
    }
}

export default LobbyHub;