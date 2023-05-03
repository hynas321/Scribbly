import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface Player {
  username: string;
  score: number;
  token: string
};

const randomNumber = Math.floor(Math.random() * 10000);

const initialState: Player = {
  username: `Player ${randomNumber}`,
  score: 0,
  token: localStorage.getItem("token") ?? "",
};

const playerSlice = createSlice({
  name: "player",
  initialState,
  reducers: {
    updatedUsername(state, action: PayloadAction<string>) {
      state.username = action.payload;
    },
    updatedScore(state, action: PayloadAction<number>) {
      state.score += action.payload;
    },
    updatedToken(state, action: PayloadAction<string>) {
      state.token = action.payload;
      localStorage.setItem("token", action.payload);
    },
    updatedPlayer(state, action: PayloadAction<Player>) {
      state.username = action.payload.username;
      state.score = action.payload.score;
      state.token = action.payload.token;
      
      localStorage.setItem("token", action.payload.token);
    }
  }
})

export const { updatedUsername, updatedScore, updatedToken, updatedPlayer } = playerSlice.actions;
export default playerSlice.reducer;