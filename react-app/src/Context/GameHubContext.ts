import { createContext } from "react";
import config from '../../config.json'
import Hub from '../hub/Hub';

export const gameHub: Hub = new Hub(`${config.httpServerUrl}${config.hubGameEndpoint}`);
export const GameHubContext = createContext<Hub>(gameHub);