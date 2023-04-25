import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface Player {
  username: string;
  token: string;
  gameHash: string;
  score: number;
};

const randomNumber = Math.floor(Math.random() * 10000);

const initialState: Player = {
  username: `Test ${randomNumber}`,
  token: "TestToken",
  gameHash: "TestHash",
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
      state.username = action.payload;
    },
    updatedGameHash(state, action: PayloadAction<string>) {
      state.gameHash = action.payload;
    },
    updatedScore(state, action: PayloadAction<number>) {
      state.score += action.payload;
    }
  }
})

export const { updatedUsername, updatedToken, updatedGameHash, updatedScore } = playerSlice.actions;
export default playerSlice.reducer;