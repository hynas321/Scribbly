import { createContext } from "react";
import config from '../../config.json'
import Hub from '../hub/Hub';
import ApiEndpoints from "../http/ApiEndpoints";

export const connectionHub: Hub = new Hub(`${config.httpServerUrl}${ApiEndpoints.hubConnectionEndpoint}`);
export const longRunningConnectionHub: Hub = new Hub(`${config.httpServerUrl}${ApiEndpoints.longRunningHubConnectionEndpoint}`);
export const accountConnectionHub: Hub = new Hub(`${config.httpServerUrl}${ApiEndpoints.accountHubConnectionEndpoint}`);
export const ConnectionHubContext = createContext<Hub>(connectionHub);
export const LongRunningConnectionHubContext = createContext<Hub>(longRunningConnectionHub);
export const AccountHubContext = createContext<Hub>(accountConnectionHub);