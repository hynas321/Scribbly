interface CreateGameRequestBody {
  hostUsername: string;
}
  
interface CreateLobbyRequestResponse {
  hostToken: string;
  gameHash: string
}

interface LobbyExistsRequestBody {
  
}