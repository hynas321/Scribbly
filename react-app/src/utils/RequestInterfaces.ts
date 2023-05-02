import { Player } from "../redux/slices/player-slice";

export interface CreateGameBody {
  username: string;
}

export interface CreateGameResponse {
  gameHash: string;
  hostToken: string;
}

export interface CreateGameRequestResponse {
  gameHash: string;
  hostToken: string;
}

export interface GameExistsBody {
  gameHash: string;
} 

export interface JoinGameRequestBody {
  gameHash: string;
  username: string;
}

export interface JoinGameRequestResponse {
  player: Player;
  playerList: PlayerScore[];
  gameIsStarted: boolean;
}

export interface PlayerIsHostBody {
  gameHash: string
  token: string,
}