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

  async joinGame(gameHash: string, token: string, username: string): Promise<any> {
    const requestBody: JoinGameBody = {
      username: username
    };

    try {
      const response = await fetch(`${this.httpServerUrl}${ApiEndpoints.playerJoinGame}/${gameHash}`, {
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

  async checkIfGameIsStarted(gameHash: string): Promise<any> {
    try {
      const response = await fetch(`${this.httpServerUrl}${ApiEndpoints.gameIsStarted}/${gameHash}`);

      if (!response.ok) {
        throw new Error("Error");
      }

      return await response.json() as boolean;
    } 
    catch (error) {
      return error;
    }
  }

  async checkIfGameExists(gameHash: string): Promise<any> {
    try {
      const response = await fetch(`${this.httpServerUrl}${ApiEndpoints.gameExists}/${gameHash}`);

      if (!response.ok) {
        throw new Error("Error");
      }

      return await response.json();
    }
    catch (error) {
      return error;
    }
  }
  
  async checkIfPlayerExists(gameHash: string, token: string): Promise<any> {
    try {
      const response = await fetch(`${this.httpServerUrl}${ApiEndpoints.playerExists}/${gameHash}`, {
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

  async fetchTopAccountScores(): Promise<any> {
    try {
      const response = await fetch(`${this.httpServerUrl}${ApiEndpoints.accountGetTopScores}`);

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

  async updateAccountScore(gameHash: string, token: string, accessToken: string): Promise<any> {
    try {
      const response = await fetch(`${this.httpServerUrl}${ApiEndpoints.accountIncrementScore}/${gameHash}`, {
        method: 'POST',
        headers: {
          'Token': token,
          'AccessToken': accessToken
        }
      });

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