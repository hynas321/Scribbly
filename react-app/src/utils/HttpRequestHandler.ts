import axios from 'axios';
import config from '../../config.json';
import ApiEndpoints from '../hub/ApiEndpoints';

class HttpRequestHandler {
  private httpServerUrl: string = config.httpServerUrl;

  async createGame(hostUsername: string): Promise<any> {
    const requestBody: CreateGameRequestBody = {
      hostUsername: hostUsername
    };

    return await axios.post(`${this.httpServerUrl}${ApiEndpoints.gameCreate}`, requestBody)
      .then(response => {
        switch (response.status) {
          case 201:
            return response.data
          default:
            throw new Error("Error");
        }
      })
      .catch(error => {
        return error;
      });
  }

  async joinGame(gameHash: string, username: string): Promise<any> {
    const requestBody: JoinGameRequestBody = {
      gameHash: gameHash,
      username: username
    };

    return await axios.post(`${this.httpServerUrl}${ApiEndpoints.playerJoinGame}`, requestBody)
      .then(response => {
        switch (response.status) {
          case 201:
            console.log(response.data)
            return response.data
          default:
            throw new Error("Error");
        }
      })
      .catch(error => {
        return error;
      });
  }

  async gameExists(gameHash: string): Promise<any> {
    const requestBody: LobbyExistsRequestBody = {
      gameHash: gameHash as string
    }

    return await axios.post(`${this.httpServerUrl}${ApiEndpoints.gameExists}`, requestBody)
      .then(response => {
        switch (response.status) {
          case 200:
            return response.data as boolean
          default:
            throw new Error("Error");
        }
      })
      .catch(error => {
        return error;
      });
  }

  async fetchPlayerScores()
  {
    return await axios.get(`${this.httpServerUrl}${ApiEndpoints.playerScoresGet}`)
      .then(response => {
        switch (response.status) {
          case 200:
            return response.data as PlayerScore[];
          default:
            throw new Error("Error");
        }
      })
      .catch(error => {
        return error;
      });
  }
}

export default HttpRequestHandler;