import axios from 'axios';
import config from '../../config.json';
import { CreateGameBody, CreateGameResponse, JoinGameBody, JoinGameResponse, PlayerIsHostResponse } from './HttpInterfaces';
import ApiEndpoints from '../hub/ApiEndpoints';


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

  async joinGame(token: string, username: string): Promise<any> {
    const requestBody: JoinGameBody = {
      username: username
    };

    return await axios.post<JoinGameResponse>(`${this.httpServerUrl}${ApiEndpoints.playerJoinGame}`, requestBody, {
        headers: {
          "Token": token,
        }
      })
      .then(response => {
        switch (response.status) {
          case 200:
            return response.data
          default:
            throw new Error("Error");
        }
      })
      .catch(error => {
        return error;
      });
  }

  async checkIfGameIsStarted(): Promise<any> {
    return await axios.get<boolean>(`${this.httpServerUrl}${ApiEndpoints.gameIsStarted}`)
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

  async checkIfGameExists(): Promise<any> {

    return await axios.get<boolean>(`${this.httpServerUrl}${ApiEndpoints.gameExists}`)
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

  async checkIfPlayerExists(token: string): Promise<any> {
    return await axios.get<boolean>(`${this.httpServerUrl}${ApiEndpoints.playerExists}`, {
      headers: {
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

  async checkIfPlayerIsHost(token: string): Promise<PlayerIsHostResponse> {
    const response = await fetch(`${this.httpServerUrl}${ApiEndpoints.playerIsHost}`, {
      headers: {
        'Token': token
      }
    });
  
    if (!response.ok) {
      throw new Error("Error");
    }
  
    const data = await response.json();
    return data as PlayerIsHostResponse;
  }

  async fetchPlayerScores() {
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