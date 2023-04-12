import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface Player {
  username: string;
  points: number;
};

const initialState: Player = {
  username: "Test",
  points: 0
};

const playerSlice = createSlice({
  name: "player",
  initialState,
  reducers: {
    updatedUsername(state, action: PayloadAction<string>) {
      state.username = action.payload;
    },
    updatedPoints(state, action: PayloadAction<number>) {
      state.points += action.payload;
    }
  }
})

export const { updatedUsername, updatedPoints } = playerSlice.actions;
export default playerSlice.reducer;