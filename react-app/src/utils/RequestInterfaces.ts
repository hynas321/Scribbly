interface CreateGameRequestBody {
  hostUsername: string;
}

interface CreateGameRequestResponse {
  gameHash: string;
  hostToken: string;
}

interface JoinGameRequestBody {
  gameHash: string;
  username: string;
}

interface LobbyExistsRequestBody {
  
}