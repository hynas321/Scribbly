import { createContext } from "react";
import config from '../../config.json'
import Hub from '../hub/Hub';

export const lobbyHub: Hub = new Hub(`${config.httpServerUrl}${config.hubLobbyEndpoint}`);
export const LobbyHubContext = createContext<Hub>(lobbyHub);