import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface GameSettings {
  drawingTimeSeconds: number;
  roundsCount: number;
  wordLanguage: string;
};

const initialState: GameSettings = {
  drawingTimeSeconds: 75,
  roundsCount: 6,
  wordLanguage: "en"
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
    updatedWordLanguage(state, action: PayloadAction<string>) {
      state.wordLanguage = action.payload;
    },
    updatedGameSettings(state, action: PayloadAction<GameSettings>) {
      state.drawingTimeSeconds = action.payload.drawingTimeSeconds,
      state.roundsCount = action.payload.roundsCount,
      state.wordLanguage = action.payload.wordLanguage
    }
  }
})

export const { updatedDrawingTimeSeconds, updatedRoundsCount, updatedWordLanguage, updatedGameSettings } = gameSettingsSlice.actions;
export default gameSettingsSlice.reducer;