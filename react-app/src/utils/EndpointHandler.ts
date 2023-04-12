import axios from 'axios';
import config from '../../config.json';
import { GameSettings } from '../redux/slices/game-settings-slice';

class EndpointHandler {
  private serverUrl: string = config.serverUrl;

  async createGame(
    endpoint: string,
    hostUsername: string,
    gameSettings: GameSettings): Promise<any> {

    const requestBody: CreateGameRequestBody = {
        hostUsername: hostUsername,
        nonAbstractNounsOnly: gameSettings.nonAbstractNounsOnly,
        drawingTimespanSeconds: gameSettings.drawingTimeSeconds,
        roundsCount: gameSettings.roundsCount
    }

    return await axios.post(`${this.serverUrl}${endpoint}`, requestBody)
    .then(response => {
        return response;
    })
    .catch(error => {
        return error;
    });
  }
}

interface CreateGameRequestBody {
  hostUsername: string,
  nonAbstractNounsOnly: boolean,
  drawingTimespanSeconds: number,
  roundsCount: number
};

export default EndpointHandler;