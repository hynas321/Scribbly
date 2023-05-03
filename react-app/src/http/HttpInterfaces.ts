export interface CreateGameBody {
  username: string;
}

export interface CreateGameResponse {
  hostToken: string;
}

export interface JoinGameBody {
  username: string;
}

export interface JoinGameResponse {
  playerScores: PlayerScore[];
}

export interface PlayerIsHostResponse {
  isHost: boolean;
}
