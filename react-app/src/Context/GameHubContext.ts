import { createContext } from "react";
import config from '../../config.json'
import Hub from '../Hubs/Hub';

export const gameHub: Hub = new Hub(`${config.httpServerUrl}${config.hubGameEndpoint}`);
export const GameHubContext = createContext<Hub>(gameHub);