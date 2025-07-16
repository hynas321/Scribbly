import axios from "axios";
import ApiEndpoints from "./ApiEndpoints";
import { CreateGameBody } from "./HttpInterfaces";
import { MainScoreboardScore } from "../interfaces/MainScoreboardScore";
import { Account } from "../interfaces/Account";

const httpServerUrl: string = import.meta.env.VITE_SERVER_URL;

const api = {
  async createGame(username: string): Promise<any> {
    const requestBody: CreateGameBody = { username };

    try {
      const response = await axios.post(
        `${httpServerUrl}${ApiEndpoints.gameCreate}`,
        requestBody,
        { headers: { "Content-Type": "application/json" } }
      );

      if (response.status !== 201) {
        throw new Error("Error creating game");
      }

      return response.data;
    } catch (error: any) {
      return error;
    }
  },

  async checkIfGameExists(gameHash: string): Promise<any> {
    try {
      const response = await axios.get(
        `${httpServerUrl}${ApiEndpoints.gameExists}/${gameHash}`
      );

      if (response.status !== 200) {
        throw new Error("Error checking game existence");
      }

      return response.data;
    } catch (error: any) {
      return error;
    }
  },

  async fetchTopAccountScores(): Promise<MainScoreboardScore[]> {
    try {
      const response = await axios.get(
        `${httpServerUrl}${ApiEndpoints.accountGetTopScores}`
      );

      if (response.status !== 200) {
        throw new Error("Error fetching top scores");
      }

      return response.data;
    } catch (error: any) {
      return error;
    }
  },

  async addAccountIfNotExists(profileObj: any, accessToken: string): Promise<any> {
    try {
      const account: Account = {
        id: profileObj.googleId,
        accessToken,
        email: profileObj.email,
        name: profileObj.name,
        givenName: profileObj.givenName,
        familyName: profileObj.familyName,
        score: 0,
      };

      const response = await axios.post(
        `${httpServerUrl}${ApiEndpoints.accountAddIfNotExists}`,
        { account },
        { headers: { "Content-Type": "application/json" } }
      );

      if (response.status !== 200) {
        throw new Error("Error adding account");
      }

      return response.status;
    } catch (error: any) {
      return error;
    }
  },

  async fetchAccountScore(id: string): Promise<number> {
    try {
      const response = await axios.get(
        `${httpServerUrl}${ApiEndpoints.accountGetScore}/${id}`
      );

      if (response.status !== 200) {
        throw new Error("Error fetching account score");
      }

      return response.data;
    } catch (error: any) {
      return error;
    }
  },

  async updateAccountScore(gameHash: string, token: string, accessToken: string): Promise<number> {
    try {
      const response = await axios.put(
        `${httpServerUrl}${ApiEndpoints.accountIncrementScore}/${gameHash}`,
        undefined,
        {
          headers: {
            Token: token,
            AccessToken: accessToken,
          },
        }
      );

      if (response.status !== 200) {
        throw new Error("Error updating account score");
      }

      return response.data;
    } catch (error: any) {
      return error;
    }
  },
};

export default api;
