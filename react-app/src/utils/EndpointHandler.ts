import axios from 'axios'
import config from '../../config.json'
import { GameSettings } from '../redux/slices/game-settings-slice'

class EndpointHandler {
    private serverUrl: string = config.serverUrl

    createGame(
        endpoint: string,
        hostUsername: string,
        gameSettings: GameSettings) {

        const requestBody: CreateGameRequestBody = {
            hostUsername: hostUsername,
            nonAbstractNounsOnly: gameSettings.nonAbstractNounsOnly,
            drawingTimespanSeconds: gameSettings.drawingTimespanSeconds,
            roundsCount: gameSettings.roundsCount
        }

        axios.post(`${this.serverUrl}${endpoint}`, requestBody)
        .then(response => {
            console.log(response);
        })
        .catch(error => {
            console.error(error);
        });
    }
}

interface CreateGameRequestBody {
    hostUsername: string,
    nonAbstractNounsOnly: boolean,
    drawingTimespanSeconds: number,
    roundsCount: number
}

export default EndpointHandler