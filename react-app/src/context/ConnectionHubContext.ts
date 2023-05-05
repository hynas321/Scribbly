import { createContext } from "react";
import config from '../../config.json'
import Hub from '../hub/Hub';
import ApiEndpoints from "../hub/HttpEndpoints";

export const connectionHub: Hub = new Hub(`${config.httpServerUrl}${ApiEndpoints.hubConnectionEndpoint}`);
export const ConnectionHubContext = createContext<Hub>(connectionHub);