import { createContext } from "react";
import Hub from '../hub/Hub';
import ApiEndpoints from "../http/ApiEndpoints";

export const connectionHub: Hub = new Hub(ApiEndpoints.hubConnectionEndpoint);
export const longRunningConnectionHub: Hub = new Hub(ApiEndpoints.longRunningHubConnectionEndpoint);
export const accountConnectionHub: Hub = new Hub(ApiEndpoints.accountHubConnectionEndpoint);

export const ConnectionHubContext = createContext<Hub>(connectionHub);
export const LongRunningConnectionHubContext = createContext<Hub>(longRunningConnectionHub);
export const AccountHubContext = createContext<Hub>(accountConnectionHub);