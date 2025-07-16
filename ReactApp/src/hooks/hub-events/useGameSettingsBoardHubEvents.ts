import { useEffect } from "react";
import * as signalR from "@microsoft/signalr";
import HubEvents from "../../hub/HubMessages";
import { GameSettings, updatedDrawingTimeSeconds, updatedRoundsCount, updatedWordLanguage } from "../../redux/slices/game-settings-slice";
import Hub from "../../hub/Hub";
import { useDispatch } from "react-redux";
import { useSessionStorage } from "../useSessionStorage";

export const useGameSettingsBoardHubEvents = (hub: Hub, gameHash: string) => {
  const dispatch = useDispatch();
  const { authorizationToken } = useSessionStorage();
  
  useEffect(() => {
    if (hub.getState() != signalR.HubConnectionState.Connected || !gameHash) {
      return;
    }

    hub.on(HubEvents.onLoadGameSettings, (gameSettingsSerialized: any) => {
      const gameSettings = JSON.parse(gameSettingsSerialized) as GameSettings;

      dispatch(updatedDrawingTimeSeconds(gameSettings.drawingTimeSeconds));
      dispatch(updatedRoundsCount(gameSettings.roundsCount));
      dispatch(updatedWordLanguage(gameSettings.wordLanguage));
    });

    hub.on(HubEvents.onSetDrawingTimeSeconds, (value: number) => {
      dispatch(updatedDrawingTimeSeconds(value));
    });

    hub.on(HubEvents.onSetRoundsCount, (value: number) => {
      dispatch(updatedRoundsCount(value));
    });

    hub.on(HubEvents.onSetWordLanguage, (value: string) => {
      dispatch(updatedWordLanguage(value));
    });

    (async () => {
      await hub.invoke(HubEvents.LoadGameSettings, gameHash, authorizationToken);
    })();

    return () => {
      hub.off(HubEvents.onLoadGameSettings);
      hub.off(HubEvents.onSetDrawingTimeSeconds);
      hub.off(HubEvents.onSetRoundsCount);
      hub.off(HubEvents.onSetWordLanguage);
    };
  }, [hub.getState(), gameHash]);  
}