import { createContext } from "react";
import Hub from "../hub/Hub";
import ApiEndpoints from "../http/ApiEndpoints";
import { sessionStore } from "../session/sessionStore";

export const connectionHub = new Hub(ApiEndpoints.hubConnectionEndpoint, undefined, () =>
  sessionStore.get("roomAccessToken"),
);

export const ConnectionHubContext = createContext<Hub>(connectionHub);
