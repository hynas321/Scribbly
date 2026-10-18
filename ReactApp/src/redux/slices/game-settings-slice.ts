import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import config from "../../../config.json";

export type HintSpeed = "off" | "slow" | "normal" | "fast";

export const hintPercentageOptions = Array.from(
  {
    length:
      (config.gameSettings.maxHintPercentage - config.gameSettings.minHintPercentage) /
        config.gameSettings.hintPercentageStep +
      1,
  },
  (_, index) =>
    config.gameSettings.minHintPercentage + index * config.gameSettings.hintPercentageStep,
);

export const normalizeHintPercentage = (value: number) => {
  const { minHintPercentage, maxHintPercentage, hintPercentageStep } = config.gameSettings;
  const normalized =
    Math.floor((value - minHintPercentage) / hintPercentageStep) * hintPercentageStep +
    minHintPercentage;

  return Math.min(maxHintPercentage, Math.max(minHintPercentage, normalized));
};

export interface GameSettings {
  drawingTimeSeconds: number;
  roundsCount: number;
  promptLanguage: string;
  maxPlayers: number;
  promptCategories: string[];
  hintSpeed: HintSpeed;
  hintPercentage: number;
}

const initialState: GameSettings = {
  drawingTimeSeconds: 75,
  roundsCount: 6,
  promptLanguage: "en",
  maxPlayers: 6,
  promptCategories: [],
  hintSpeed: "off",
  hintPercentage: 25,
};

const gameSettingsSlice = createSlice({
  name: "gameSettings",
  initialState,
  reducers: {
    updatedDrawingTimeSeconds(state, action: PayloadAction<number>) {
      state.drawingTimeSeconds = action.payload;
    },
    updatedRoundsCount(state, action: PayloadAction<number>) {
      state.roundsCount = action.payload;
    },
    updatedPromptLanguage(state, action: PayloadAction<string>) {
      state.promptLanguage = action.payload;
    },
    updatedMaxPlayers(state, action: PayloadAction<number>) {
      state.maxPlayers = action.payload;
    },
    updatedPromptCategories(state, action: PayloadAction<string[]>) {
      state.promptCategories = action.payload;
    },
    updatedHintSpeed(state, action: PayloadAction<HintSpeed>) {
      state.hintSpeed = action.payload;
    },
    updatedHintPercentage(state, action: PayloadAction<number>) {
      state.hintPercentage = normalizeHintPercentage(action.payload);
    },
    updatedGameSettings(state, action: PayloadAction<GameSettings>) {
      state.drawingTimeSeconds = action.payload.drawingTimeSeconds;
      state.roundsCount = action.payload.roundsCount;
      state.promptLanguage = action.payload.promptLanguage;
      state.maxPlayers = action.payload.maxPlayers ?? 6;
      state.promptCategories = action.payload.promptCategories ?? [];
      state.hintSpeed = action.payload.hintSpeed ?? "off";
      state.hintPercentage = normalizeHintPercentage(action.payload.hintPercentage ?? 25);
    },
  },
});

export const {
  updatedDrawingTimeSeconds,
  updatedRoundsCount,
  updatedPromptLanguage,
  updatedMaxPlayers,
  updatedPromptCategories,
  updatedHintSpeed,
  updatedHintPercentage,
  updatedGameSettings,
} = gameSettingsSlice.actions;
export default gameSettingsSlice.reducer;
