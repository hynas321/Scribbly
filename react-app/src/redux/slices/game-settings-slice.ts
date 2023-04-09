import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface GameSettings {
  nonAbstractNounsOnly: boolean;
  drawingTimespanSeconds: number;
  roundsCount: number;
};

const initialState: GameSettings = {
  nonAbstractNounsOnly: true,
  drawingTimespanSeconds: 75,
  roundsCount: 4
};

const gameSettingsSlice = createSlice({
  name: "gameSettings",
  initialState,
  reducers: {
    updatedNonAbstractNounsOnly(state, action: PayloadAction<boolean>) {
      state.nonAbstractNounsOnly = action.payload;
    },
    updatedDrawingTimespanSeconds(state, action: PayloadAction<number>) {
      state.drawingTimespanSeconds = action.payload;
    },
    updatedRoundsCount(state, action: PayloadAction<number>) {
      state.roundsCount = action.payload;
    }
  }
})

export const { updatedNonAbstractNounsOnly, updatedDrawingTimespanSeconds, updatedRoundsCount } = gameSettingsSlice.actions;
export default gameSettingsSlice.reducer;