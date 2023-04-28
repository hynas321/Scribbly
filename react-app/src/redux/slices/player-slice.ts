import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface Player {
  username: string;
  token: string;
  score: number;
};

const randomNumber = Math.floor(Math.random() * 10000);

const initialState: Player = {
  username: `Test ${randomNumber}`,
  token: "TestToken",
  score: 0
};

const playerSlice = createSlice({
  name: "player",
  initialState,
  reducers: {
    updatedUsername(state, action: PayloadAction<string>) {
      state.username = action.payload;
    },
    updatedToken(state, action: PayloadAction<string>) {
      state.token = action.payload;
    },
    updatedScore(state, action: PayloadAction<number>) {
      state.score += action.payload;
    },
    updatedPlayer(state, action: PayloadAction<Player>) {
      state.username = action.payload.username;
      state.token = action.payload.token;
      state.score = action.payload.score;
    }
  }
})

export const { updatedUsername, updatedToken, updatedScore, updatedPlayer } = playerSlice.actions;
export default playerSlice.reducer;