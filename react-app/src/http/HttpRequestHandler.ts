import config from '../../config.json';
import ApiEndpoints from './ApiEndpoints';
import { CreateGameBody, JoinGameBody, PlayerIsHostResponse, UsernameExistsBody } from './HttpInterfaces';

class HttpRequestHandler {
  private httpServerUrl: string = config.httpServerUrl;

  async createGame(username: string): Promise<any> {
    const requestBody: CreateGameBody = {
      username: username
    };

    try {
      const response = await fetch(`${this.httpServerUrl}${ApiEndpoints.gameCreate}`, {
        method: 'POST',
        body: JSON.stringify(requestBody),
        headers: {
          'Content-Type': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error("Error");
      } 

      return await response.json();
    }
    catch (error) {
      return error;
    }
  }

  async joinGame(token: string, username: string): Promise<any> {
    const requestBody: JoinGameBody = {
      username: username
    };

    try {
      const response = await fetch(`${this.httpServerUrl}${ApiEndpoints.playerJoinGame}`, {
        method: 'POST',
        body: JSON.stringify(requestBody),
        headers: {
          'Content-Type': 'application/json',
          'Token': token
        }
      });

      if (!response.ok) {
        throw new Error("Error");
      }

      return await response.json();
    }
    catch (error) {
      return error;
    }
  }

  async checkIfGameIsStarted(): Promise<any> {
    try {
      const response = await fetch(`${this.httpServerUrl}${ApiEndpoints.gameIsStarted}`);

      if (!response.ok) {
        throw new Error("Error");
      }

      return await response.json() as boolean;
    } 
    catch (error) {
      return error;
    }
  }

  async checkIfGameExists(): Promise<any> {
    try {
      const response = await fetch(`${this.httpServerUrl}${ApiEndpoints.gameExists}`);

      if (!response.ok) {
        throw new Error("Error");
      }

      return await response.json();
    }
    catch (error) {
      return error;
    }
  }
  
  async fetchGameHash(token: string): Promise<any> {
    try {
      const response = await fetch(`${this.httpServerUrl}${ApiEndpoints.gameGetHash}`, {
        headers: {
          'Token': token
        }
      });
  
      if (!response.ok) {
        throw new Error("Error");
      }

      return await response.text();

    }
    catch (error) {
      return error;
    }
  }
  
  async checkIfPlayerExists(token: string): Promise<any> {
    try {
      const response = await fetch(`${this.httpServerUrl}${ApiEndpoints.playerExists}`, {
        headers: {
          'Token': token
        }
      });
  
      if (!response.ok) {
        throw new Error("Error");
      }

      return await response.json() as boolean;
    } 
    catch (error) {
      return error;
    }
  }
  
  async checkIfPlayerIsHost(token: string): Promise<any> {
    try {
      const response = await fetch(`${this.httpServerUrl}${ApiEndpoints.playerHost}`, {
        headers: {
          'Token': token
        }
      });

      if (!response.ok) {
        throw new Error("Error");
      }

      return await response.json() as PlayerIsHostResponse;
    }
    catch (error) {
      return error;
    }
  }

  async checkIfUsernameExists(username: string): Promise<any> {
    try {
      const response = await fetch(`${this.httpServerUrl}${ApiEndpoints.playerUsernameExists}${username}`);

      if (!response.ok) {
        throw new Error("Error");
      }

      return await response.json() as boolean;
    }
    catch (error) {
      return error;
    }
  }

  async fetchPlayerScores(): Promise<any> {
    try {
      const response = await fetch(`${this.httpServerUrl}${ApiEndpoints.scoreboardGet}`);

      if (!response.ok) {
        throw new Error("Error");
      }

      return await response.json() as MainScoreboardScore[];
    }
    catch (error) {
      return error;
    }
  }

  async addAccountIfNotExists(profileObj: any, accessToken: string): Promise<any> {
    try {
      const account: Account = {
        id: profileObj.googleId,
        accessToken: accessToken,
        email: profileObj.email,
        name: profileObj.name,
        givenName: profileObj.givenName,
        familyName: profileObj.familyName,
        score: 0
      };

      const requestBody = {
        account: account
      };

      const response = await fetch(`${this.httpServerUrl}${ApiEndpoints.accountAddIfNotExists}`, {
        method: 'POST',
        body: JSON.stringify(requestBody),
        headers: {
          'Content-Type': 'application/json',
        }
      });

      if (!response.ok) {
        throw new Error("Error");
      }

      return await response.json();
    }
    catch (error) {
      return error;
    }
  }

  async fetchAccountScore(id: string): Promise<any> {
    try {
      const response = await fetch(`${this.httpServerUrl}${ApiEndpoints.accountGetScore}/${id}`);

      if (!response.ok) {
        throw new Error("Error");
      }

      return await response.json() as number;
    }
    catch (error) {
      return error;
    }
  }
}

export default HttpRequestHandler;