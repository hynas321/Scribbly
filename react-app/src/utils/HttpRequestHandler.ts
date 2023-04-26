import axios from 'axios';
import config from '../../config.json';
import { GameSettings } from '../redux/slices/game-settings-slice';

class HttpRequestHandler {
  private httpServerUrl: string = config.httpServerUrl;

  async createGame(
    hostUsername: string,
    gameSettings: GameSettings): Promise<any> {

    const requestBody: CreateGameRequestBody = {
        hostUsername: hostUsername,
        nonAbstractNounsOnly: gameSettings.nonAbstractNounsOnly,
        drawingTimeSeconds: gameSettings.drawingTimeSeconds,
        roundsCount: gameSettings.roundsCount,
        wordLanguage: gameSettings.wordLanguage
    }

    return await axios.post(`${this.httpServerUrl}${config.createGameServerEndpoint}`, requestBody)
    .then(response => {
      return response;
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

interface CreateGameRequestBody {
  hostUsername: string;
  nonAbstractNounsOnly: boolean;
  drawingTimeSeconds: number;
  roundsCount: number;
  wordLanguage: string;
};

export default HttpRequestHandler;