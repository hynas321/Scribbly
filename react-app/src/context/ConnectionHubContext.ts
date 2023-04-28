import { createContext } from "react";
import config from '../../config.json'
import Hub from '../hub/Hub';

export const connectionHub: Hub = new Hub(`${config.httpServerUrl}${config.hubConnectionEndpoint}`);
export const ConnectionHubContext = createContext<Hub>(connectionHub);