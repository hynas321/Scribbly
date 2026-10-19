export interface CreateGameResponse {
  gameHash: string;
  roomAccessToken: string;
  maxPlayers: number;
}
export interface JoinCredentialResponse {
  roomAccessToken: string;
  expiresAtUtc: string;
  maxPlayers: number;
}
export interface CompletedGameResult {
  completed: boolean;
  score: number | null;
  scores: CompletedPlayerScore[];
}

export interface CompletedPlayerScore {
  username: string | null;
  score: number;
  isCurrentPlayer: boolean;
}
