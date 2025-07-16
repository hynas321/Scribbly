import { useEffect } from "react";
import HubEvents from "../../hub/HubMessages";
import { useDispatch } from "react-redux";
import { updatedIsTimerVisible, updatedCurrentDrawingTimeSeconds } from "../../redux/slices/game-state-slice";
import { AnnouncementMessage } from "../../interfaces/AnnouncementMessage";
import Hub from "../../hub/Hub";
import * as signalR from "@microsoft/signalr";
import { useAppSelector } from "../../redux/hooks";

export const useCanvasHubEvents = (
    hub: Hub,
    setCanvasTitle: (value: AnnouncementMessage) => void,
    setIsPlayerDrawing: (value: boolean) => void
  ) => {
  const player = useAppSelector(state => state.player);
  const dispatch = useDispatch();

  useEffect(() => {
    if (hub.getState() !== signalR.HubConnectionState.Connected) {
      return;
    }

    hub.on(HubEvents.onUpdateTimer, (time: number) => {
      dispatch(updatedCurrentDrawingTimeSeconds(time));
    });

    hub.on(HubEvents.onUpdateTimerVisibility, (visible: boolean) => {
      dispatch(updatedIsTimerVisible(visible));
    });

    hub.on(HubEvents.onUpdateDrawingPlayer, (drawingPlayerUsername: string) => {
      if (drawingPlayerUsername == player.username) {
        setIsPlayerDrawing(true);
      } else {
        setIsPlayerDrawing(false);
      }
    });

    hub.on(HubEvents.OnSetCanvasText, (announcementMessageSerialized: string) => {
      const announcementMessage = JSON.parse(announcementMessageSerialized) as AnnouncementMessage;

      setCanvasTitle(announcementMessage);
    });

    return () => {
      hub.off(HubEvents.onUpdateTimer);
      hub.off(HubEvents.onUpdateTimerVisibility);
      hub.off(HubEvents.onUpdateDrawingPlayer);
      hub.off(HubEvents.OnSetCanvasText);
    };
  }, [hub.getState()]);
} 
