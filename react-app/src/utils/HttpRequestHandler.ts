import axios from 'axios';
import config from '../../config.json';
import { GameSettings } from '../redux/slices/game-settings-slice';

class HttpRequestHandler {
  private httpServerUrl: string = config.httpServerUrl;

  async createGame(hostUsername: string): Promise<any> {
    const requestBody: CreateGameRequestBody = {
      hostUsername: hostUsername
    };

    return await axios.post(`${this.httpServerUrl}${config.createGameServerEndpoint}`, requestBody)
      .then(response => {
        return response;
      })
      .catch(error => {
        return error;
      });
  }

  async gameExists(gameHash: string): Promise<any> {
    const requestBody: LobbyExistsRequestBody = {
      gameHash: gameHash as string
    }

    return await axios.post(`${this.httpServerUrl}${config.gameExists}`, requestBody)
      .then(response => {
        switch (response.status) {
          case 200:
            return response.data as boolean
          case 500:
            throw new Error("Could not check whether the lobby exists");
        }
      })
      .catch(error => {
        return error;
      });
  }

  async fetchPlayerScores()
  {
    return await axios.get(`${this.httpServerUrl}${config.fetchPlayerScoresServerEndpoint}`)
      .then(response => {
        switch (response.status) {
          case 200:
            return response.data as PlayerScore[];
          case 500:
            throw new Error("Could not fetch player scores");
        }
      })
      .catch(error => {
        return error;
      });
  }
}

export default HttpRequestHandler;