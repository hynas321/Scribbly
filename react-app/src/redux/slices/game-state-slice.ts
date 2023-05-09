import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface GameState {
  currentDrawingTimeSeconds: number;
  currentRound: number;
  secretWord: string;
  drawingPlayerUsername: string;
  hostPlayerUsername: string;
  isGameStarted: boolean;
};

const initialState: GameState = {
  currentDrawingTimeSeconds: 50,
  currentRound: 1,
  secretWord: "",
  drawingPlayerUsername: "",
  hostPlayerUsername: "",
  isGameStarted: false
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
    updatedWordLength(state, action: PayloadAction<string>) {
      state.secretWord = action.payload;
    },
    updatedDrawingPlayerUsername(state, action: PayloadAction<string>) {
      state.drawingPlayerUsername = action.payload;
    },
    updatedHostPlayerUsername(state, action: PayloadAction<string>) {
      state.hostPlayerUsername = action.payload;
    },
    updatedIsGameStarted(state, action: PayloadAction<boolean>) {
      state.isGameStarted = action.payload;
    },
    updatedGameState(state, action: PayloadAction<GameState>) {
      state.currentDrawingTimeSeconds = action.payload.currentDrawingTimeSeconds,
      state.currentRound = action.payload.currentRound,
      state.secretWord = action.payload.secretWord,
      state.drawingPlayerUsername = action.payload.drawingPlayerUsername,
      state.hostPlayerUsername = action.payload.hostPlayerUsername,
      state.isGameStarted = action.payload.isGameStarted
    },
    clearedGameState(state) {
      state.currentDrawingTimeSeconds = initialState.currentDrawingTimeSeconds,
      state.currentRound = initialState.currentRound,
      state.secretWord = initialState.secretWord,
      state.drawingPlayerUsername = initialState.drawingPlayerUsername,
      state.hostPlayerUsername = initialState.hostPlayerUsername,
      state.isGameStarted = initialState.isGameStarted
    }
  }
})

export const { updatedCurrentDrawingTimeSeconds, updatedCurrentRound, updatedWordLength, updatedDrawingPlayerUsername, updatedHostPlayerUsername, updatedGameState, clearedGameState } = gameStateSlice.actions;
export default gameStateSlice.reducer;