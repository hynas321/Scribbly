import { PlayerScore } from "../interfaces/PlayerScore";

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

export interface UsernameExistsBody {
  username: string;
}
