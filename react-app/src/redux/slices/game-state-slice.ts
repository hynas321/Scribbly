import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface GameState {
  currentDrawingTimeSeconds: number;
  currentRound: number;
  wordLength: number;
  playerScore: PlayerScore[];
};

const initialState: GameState = {
  currentDrawingTimeSeconds: 50,
  currentRound: 1,
  wordLength: 10,
  playerScore: []
};

const gameStateSlice = createSlice({
  name: "gameState",
  initialState,
  reducers: {
    updatedCurrentDrawingTimeSeconds(state, action: PayloadAction<number>) {
      state.currentDrawingTimeSeconds = action.payload;
    },
    updatedCurrentRound(state, action: PayloadAction<number>) {
      state.currentRound = action.payload;
    },
    updatedWordLength(state, action: PayloadAction<number>) {
      state.wordLength = action.payload;
    },
    updatedPlayerList(state, action: PayloadAction<PlayerScore[]>) {
      state.playerScore = action.payload;
    },
    addedPlayerScore(state, action: PayloadAction<PlayerScore>) {
      state.playerScore.push(action.payload);
    }
  }
})

export const { updatedCurrentDrawingTimeSeconds, updatedCurrentRound, updatedWordLength, updatedPlayerList, addedPlayerScore } = gameStateSlice.actions;
export default gameStateSlice.reducer;