import { createContext } from "react";
import config from '../../config.json'
import Hub from '../hubs/Hub';

export const lobbyHub: Hub = new Hub(`${config.httpServerUrl}${config.hubLobbyEndpoint}`);
export const LobbyHubContext = createContext<Hub>(lobbyHub);