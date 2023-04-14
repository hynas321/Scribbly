import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface Player {
  username: string;
  score: number;
};

const initialState: Player = {
  username: "Test",
  score: 0
};

const playerSlice = createSlice({
  name: "player",
  initialState,
  reducers: {
    updatedUsername(state, action: PayloadAction<string>) {
      state.username = action.payload;
    },
    updatedPoints(state, action: PayloadAction<number>) {
      state.score += action.payload;
    }
  }
})

export const { updatedUsername, updatedPoints } = playerSlice.actions;
export default playerSlice.reducer;