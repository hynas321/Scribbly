import axios from "axios";
import ApiEndpoints from "./ApiEndpoints";
import { CreateGameBody } from "./HttpInterfaces";
import { MainScoreboardScore } from "../interfaces/MainScoreboardScore";
import { Account } from "../interfaces/Account";

class HttpRequestHandler {
  private httpServerUrl: string = import.meta.env.VITE_SERVER_URL;

  async createGame(username: string): Promise<any> {
    const requestBody: CreateGameBody = {
      username: username,
    };

    try {
      const response = await axios.post(
        `${this.httpServerUrl}${ApiEndpoints.gameCreate}`,
        requestBody,
        {
          headers: {
            "Content-Type": "application/json",
          },
        }
      );

      if (response.status !== 201) {
        throw new Error("Error");
      }

      return response.data as object;
    } catch (error: any) {
      return error;
    }
  }

  async checkIfGameExists(gameHash: string): Promise<any> {
    try {
      const response = await axios.get(
        `${this.httpServerUrl}${ApiEndpoints.gameExists}/${gameHash}`
      );

      if (response.status !== 200) {
        throw new Error("Error");
      }

      return response.data as boolean;
    } catch (error: any) {
      return error;
    }
  }

  async fetchTopAccountScores(): Promise<MainScoreboardScore[]> {
    try {
      const response = await axios.get(`${this.httpServerUrl}${ApiEndpoints.accountGetTopScores}`);

      if (response.status !== 200) {
        throw new Error("Error");
      }

      return response.data as MainScoreboardScore[];
    } catch (error: any) {
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
        score: 0,
      };

      const requestBody = {
        account: account,
      };

      const response = await axios.post(
        `${this.httpServerUrl}${ApiEndpoints.accountAddIfNotExists}`,
        requestBody,
        {
          headers: {
            "Content-Type": "application/json",
          },
        }
      );

      if (response.status !== 200) {
        throw new Error("Error");
      }

      return response.status as number;
    } catch (error: any) {
      return error;
    }
  }

  async fetchAccountScore(id: string): Promise<number> {
    try {
      const response = await axios.get(
        `${this.httpServerUrl}${ApiEndpoints.accountGetScore}/${id}`
      );

      if (response.status !== 200) {
        throw new Error("Error");
      }

      return response.data as number;
    } catch (error: any) {
      return error;
    }
  }

  async updateAccountScore(gameHash: string, token: string, accessToken: string): Promise<number> {
    try {
      const response = await axios.put(
        `${this.httpServerUrl}${ApiEndpoints.accountIncrementScore}/${gameHash}`,
        undefined,
        {
          headers: {
            Token: token,
            AccessToken: accessToken,
          },
        }
      );

      if (response.status !== 200) {
        throw new Error("Error");
      }

      return response.data as number;
    } catch (error: any) {
      return error;
    }
  }
}

export default HttpRequestHandler;
