import axios from "axios";
import ApiEndpoints from "./ApiEndpoints";
import { CompletedGameResult, CreateGameResponse, JoinCredentialResponse } from "./HttpInterfaces";
import { MainScoreboardScore } from "../interfaces/MainScoreboardScore";

export interface AccountProfile {
  id: string;
  name: string;
  score: number;
}

const configuredServer = (import.meta.env.VITE_SERVER_URL ?? "").trim();
const client = axios.create({
  baseURL: configuredServer || window.location.origin,
  withCredentials: true,
  timeout: 12_000,
  headers: { "Content-Type": "application/json" },
});
const bearer = (token: string) => ({ Authorization: `Bearer ${token}` });

const api = {
  async createGame(username: string): Promise<CreateGameResponse> {
    try {
      return (await client.post<CreateGameResponse>(ApiEndpoints.gameCreate, { username })).data;
    } catch (error) {
      throw normalizeApiError(error, "Could not create game");
    }
  },
  async issueJoinCredential(gameHash: string, username: string): Promise<JoinCredentialResponse> {
    try {
      return (
        await client.post<JoinCredentialResponse>(ApiEndpoints.joinCredential(gameHash), {
          username,
        })
      ).data;
    } catch (error) {
      throw normalizeApiError(error, "Could not reserve a place in the lobby");
    }
  },
  async checkIfGameExists(gameHash: string): Promise<boolean> {
    try {
      return (
        (await client.get<boolean>(`${ApiEndpoints.gameExists}/${encodeURIComponent(gameHash)}`))
          .data === true
      );
    } catch (error) {
      throw normalizeApiError(error, "Could not check game existence");
    }
  },
  async getCompletedGameResult(gameHash: string, token: string): Promise<CompletedGameResult> {
    try {
      return (
        await client.get<CompletedGameResult>(
          `${ApiEndpoints.gameCompleted}/${encodeURIComponent(gameHash)}`,
          {
            headers: bearer(token),
          },
        )
      ).data;
    } catch (error) {
      throw normalizeApiError(error, "Could not restore completed game result");
    }
  },
  async fetchGamePlayers(
    gameHash: string,
  ): Promise<{ usernames: string[]; playerCount: number; maxPlayers: number }> {
    try {
      return (
        await client.get<{ usernames: string[]; playerCount: number; maxPlayers: number }>(
          `${ApiEndpoints.gamePlayers}/${encodeURIComponent(gameHash)}`,
        )
      ).data;
    } catch (error) {
      throw normalizeApiError(error, "Could not fetch players in the lobby");
    }
  },
  async fetchPromptCategories(lang: string): Promise<string[]> {
    try {
      return (await client.get<string[]>(ApiEndpoints.promptCategories(lang))).data;
    } catch (error) {
      throw normalizeApiError(error, "Could not fetch prompt categories");
    }
  },
  async fetchTopAccountScores(): Promise<MainScoreboardScore[]> {
    try {
      return (await client.get<MainScoreboardScore[]>(ApiEndpoints.accountGetTopScores)).data;
    } catch (error) {
      throw normalizeApiError(error, "Could not fetch top scores");
    }
  },
  async signIn(idToken: string): Promise<AccountProfile> {
    try {
      return (await client.post<AccountProfile>(ApiEndpoints.accountAddIfNotExists, { idToken }))
        .data;
    } catch (error) {
      throw normalizeApiError(error, "Could not sign in");
    }
  },
  async fetchAccountSession(): Promise<AccountProfile | null> {
    try {
      return (await client.get<AccountProfile>(ApiEndpoints.accountSession)).data;
    } catch (error) {
      if (getHttpStatus(error) === 401) {
        return null;
      }
      throw normalizeApiError(error, "Could not restore account session");
    }
  },
  async signOut(): Promise<void> {
    try {
      await client.post(ApiEndpoints.accountLogout, undefined, {
        headers: { "X-Scribbly-Request": "1" },
      });
    } catch (error) {
      throw normalizeApiError(error, "Could not sign out");
    }
  },
  async fetchAccountScore(id: string): Promise<number> {
    try {
      return (await client.get<number>(`${ApiEndpoints.accountGetScore}/${encodeURIComponent(id)}`))
        .data;
    } catch (error) {
      throw normalizeApiError(error, "Could not fetch account score");
    }
  },
  async updateAccountScore(gameHash: string, token: string): Promise<number> {
    try {
      return (
        await client.put<number>(
          `${ApiEndpoints.accountIncrementScore}/${encodeURIComponent(gameHash)}`,
          undefined,
          {
            headers: bearer(token),
          },
        )
      ).data;
    } catch (error) {
      throw normalizeApiError(error, "Could not update account score");
    }
  },
};

type HttpErrorLike = { response?: { status?: number; data?: unknown } };

function asHttpError(error: unknown): HttpErrorLike | null {
  if (typeof error !== "object" || error === null || !("response" in error)) {
    return null;
  }
  return error as HttpErrorLike;
}

function getHttpStatus(error: unknown): number | undefined {
  return asHttpError(error)?.response?.status;
}

function normalizeApiError(error: unknown, fallbackMessage: string): Error {
  const detail = asHttpError(error)?.response?.data;
  if (typeof detail === "string" && detail.length > 0) {
    return new Error(detail, { cause: error });
  }
  return error instanceof Error ? error : new Error(fallbackMessage);
}
export default api;
