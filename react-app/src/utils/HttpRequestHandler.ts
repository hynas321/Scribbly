import axios from 'axios';
import config from '../../config.json';
import ApiEndpoints from '../hub/ApiEndpoints';
import { CreateGameBody, CreateGameResponse, GameExistsBody, JoinGameRequestBody, JoinGameRequestResponse, PlayerIsHostBody } from './RequestInterfaces';
import ApiHeaders from '../hub/ApiHeaders';

class HttpRequestHandler {
  private httpServerUrl: string = config.httpServerUrl;

  async createGame(username: string): Promise<any> {
    const requestBody: CreateGameBody = {
      username: username
    };

    return await axios.post<CreateGameResponse>(`${this.httpServerUrl}${ApiEndpoints.gameCreate}`, requestBody)
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

  async checkIfGameIsStarted(gameHash: string): Promise<any> {
    return await axios.get<boolean>(`${this.httpServerUrl}${ApiEndpoints.gameIsStarted}`, {
      headers: {
        "GameHash": gameHash
      }})
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

  async joinGame(gameHash: string, username: string): Promise<any> {
    const requestBody: JoinGameRequestBody = {
      gameHash: gameHash,
      username: username
    };

    return await axios.post(`${this.httpServerUrl}${ApiEndpoints.playerJoinGame}`, requestBody)
      .then(response => {
        switch (response.status) {
          case 200:
            return response.data as JoinGameRequestResponse
          default:
            throw new Error("Error");
        }
      })
      .catch(error => {
        return error;
      });
  }

  async gameExists(gameHash: string): Promise<any> {
    const requestBody: GameExistsBody = {
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

  async fetchGameHash(token: string): Promise<any> {
    return await axios.get(`${this.httpServerUrl}${ApiEndpoints.gameGetHash}`, {
      headers: {
        'Token': token
      }
    })
    .then(response => {
      switch (response.status) {
        case 200:
          return response.data as string
        default:
          throw new Error("Error");
      }
    })
    .catch(error => {
      return error;
    });
  }

  async fetchPlayerIsHost(token: string, gameHash: string): Promise<any> {
    return await axios.get(`${this.httpServerUrl}${ApiEndpoints.playerIsHost}`, {
      headers: {
        "GameHash": gameHash,
        'Token': token
      }
    })
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
    return await axios.get(`${this.httpServerUrl}${ApiEndpoints.scoreboardGet}`)
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