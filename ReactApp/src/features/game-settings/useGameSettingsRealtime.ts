import { useEffect } from "react";
import Hub from "../../hub/Hub";
import HubEvents from "../../hub/HubMessages";
import { useAppDispatch } from "../../redux/hooks";
import {
  updatedDrawingTimeSeconds,
  updatedRoundsCount,
  updatedPromptLanguage,
  updatedMaxPlayers,
  updatedPromptCategories,
  updatedHintSpeed,
  updatedHintPercentage,
  HintSpeed,
} from "../../redux/slices/game-settings-slice";

export function useGameSettingsRealtime(hub: Hub) {
  const dispatch = useAppDispatch();

  useEffect(() => {
    const disposers = [
      hub.on(HubEvents.onDrawingTimeChanged, (value: number) => {
        dispatch(updatedDrawingTimeSeconds(value));
      }),
      hub.on(HubEvents.onRoundsCountChanged, (value: number) => {
        dispatch(updatedRoundsCount(value));
      }),
      hub.on(HubEvents.onPromptLanguageChanged, (value: string) => {
        dispatch(updatedPromptLanguage(value));
      }),
      hub.on(HubEvents.onMaxPlayersChanged, (value: number) => {
        dispatch(updatedMaxPlayers(value));
      }),
      hub.on(HubEvents.onPromptCategoriesChanged, (value: string[]) => {
        dispatch(updatedPromptCategories(value));
      }),
      hub.on(HubEvents.onHintSpeedChanged, (value: HintSpeed) => {
        dispatch(updatedHintSpeed(value));
      }),
      hub.on(HubEvents.onHintPercentageChanged, (value: number) => {
        dispatch(updatedHintPercentage(value));
      }),
    ];

    return () => {
      disposers.forEach((dispose) => dispose());
    };
  }, [hub, dispatch]);
}
