import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface GameSettings {
  nonAbstractNounsOnly: boolean;
  drawingTimeSeconds: number;
  roundsCount: number;
  wordLanguage: string;
};

const initialState: GameSettings = {
  nonAbstractNounsOnly: true,
  drawingTimeSeconds: 75,
  roundsCount: 6,
  wordLanguage: "en"
};

const gameSettingsSlice = createSlice({
  name: "gameSettings",
  initialState,
  reducers: {
    updatedNonAbstractNounsOnly(state, action: PayloadAction<boolean>) {
      state.nonAbstractNounsOnly = action.payload;
    },
    updatedDrawingTimeSeconds(state, action: PayloadAction<number>) {
      state.drawingTimeSeconds = action.payload;
    },
    updatedRoundsCount(state, action: PayloadAction<number>) {
      state.roundsCount = action.payload;
    },
    updatedWordLanguage(state, action: PayloadAction<string>) {
      state.wordLanguage = action.payload;
    }
  }
})

export const { updatedNonAbstractNounsOnly, updatedDrawingTimeSeconds, updatedRoundsCount, updatedWordLanguage } = gameSettingsSlice.actions;
export default gameSettingsSlice.reducer;