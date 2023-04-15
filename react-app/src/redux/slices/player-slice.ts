import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface Player {
  username: string;
  score: number;
  host: boolean;
};

const initialState: Player = {
  username: "Test",
  score: 0,
  host: false
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
    },
    updatedHost(state, action: PayloadAction<boolean>) {
      state.host = action.payload;
    }
  }
})

export const { updatedUsername, updatedPoints, updatedHost } = playerSlice.actions;
export default playerSlice.reducer;