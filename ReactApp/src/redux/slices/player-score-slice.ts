import { createSlice, PayloadAction } from "@reduxjs/toolkit";

export interface PlayerScore {
  username: string;
  score: number;
}

const randomNumber = Math.floor(Math.random() * 100000);

const initialState: PlayerScore = {
  username: `Player ${randomNumber}`,
  score: 0,
};

const playerScoreSlice = createSlice({
  name: "playerScore",
  initialState,
  reducers: {
    updatedUsername(state, action: PayloadAction<string>) {
      state.username = action.payload;
    },
    updatedScore(state, action: PayloadAction<number>) {
      state.score += action.payload;
    },
    updatedPlayerScore(state, action: PayloadAction<PlayerScore>) {
      state.username = action.payload.username;
      state.score = action.payload.score;
    },
  },
});

export const { updatedUsername, updatedScore, updatedPlayerScore } = playerScoreSlice.actions;
export default playerScoreSlice.reducer;
