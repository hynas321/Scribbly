import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface GameSettings {
  nonAbstractNounsOnly: boolean;
  drawingTimeSeconds: number;
  roundsCount: number;
};

const initialState: GameSettings = {
  nonAbstractNounsOnly: true,
  drawingTimeSeconds: 75,
  roundsCount: 4
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
    }
  }
})

export const { updatedNonAbstractNounsOnly, updatedDrawingTimeSeconds: updatedDrawingTimeSeconds, updatedRoundsCount } = gameSettingsSlice.actions;
export default gameSettingsSlice.reducer;