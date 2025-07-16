import { useEffect } from "react";
import HubEvents from "../../hub/HubMessages";
import { useDispatch } from "react-redux";
import { updatedCurrentRound, updatedDrawingPlayerUsername, updatedCorrectGuessPlayerUsernames } from "../../redux/slices/game-state-slice";
import Hub from "../../hub/Hub";
import * as signalR from "@microsoft/signalr";

export const usePlayerScoresHubEvents = (hub: Hub) => {
  const dispatch = useDispatch();

  useEffect(() => {
    if (hub.getState() !== signalR.HubConnectionState.Connected) {
      return;
    }

    hub.on(HubEvents.onUpdateCurrentRound, (currentRound: number) => {
      dispatch(updatedCurrentRound(currentRound));
    });

    hub.on(HubEvents.onUpdateDrawingPlayer, (drawingPlayerUsername: string) => {
      dispatch(updatedDrawingPlayerUsername(drawingPlayerUsername));
    });

    hub.on(HubEvents.onUpdateCorrectGuessPlayerUsernames, (usernamesSerialized: string) => {
      const usernames = JSON.parse(usernamesSerialized) as string[];

      dispatch(updatedCorrectGuessPlayerUsernames(usernames));
    });

    return () => {
      hub.off(HubEvents.onUpdateCurrentRound);
      hub.off(HubEvents.onUpdateDrawingPlayer);
      hub.off(HubEvents.onUpdateCorrectGuessPlayerUsernames);
    };
  }, [hub.getState()]);
} 
