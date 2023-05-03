export interface CreateGameBody {
  username: string;
}

export interface CreateGameResponse {
  gameHash: string;
  hostToken: string;
}

export interface JoinGameBody {
  username: string;
}

export interface JoinGameResponse {
  gameHash: string;
  playerScores: PlayerScore[];
}
