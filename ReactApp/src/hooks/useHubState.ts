import { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";
import Hub from "../hub/Hub";

export const useHubState = (hub: Hub): signalR.HubConnectionState => {
  const [state, setState] = useState(hub.getState());

  useEffect(() => hub.subscribeState(setState), [hub]);

  return state;
};
