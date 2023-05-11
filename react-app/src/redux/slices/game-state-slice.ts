import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface GameState {
  currentDrawingTimeSeconds: number;
  currentRound: number;
  hiddenSecretWord: string;
  drawingPlayerUsername: string;
  hostPlayerUsername: string;
  isGameStarted: boolean;
  isTimerVisible: boolean;
  correctGuessPlayerUsernames: string[];
};

const initialState: GameState = {
  currentDrawingTimeSeconds: 50,
  currentRound: 1,
  hiddenSecretWord: "? ? ?",
  drawingPlayerUsername: "",
  hostPlayerUsername: "",
  isGameStarted: false,
  isTimerVisible: false,
  correctGuessPlayerUsernames: []
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
    updatedHiddenSecretWord(state, action: PayloadAction<string>) {
      state.hiddenSecretWord = action.payload;
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
    updatedIsTimerVisible(state, action: PayloadAction<boolean>) {
      state.isTimerVisible = action.payload;
    },
    updatedCorrectGuessPlayerUsernames(state, action: PayloadAction<string[]>) {
      state.correctGuessPlayerUsernames = action.payload;
    },
    updatedGameState(state, action: PayloadAction<GameState>) {
      state.currentDrawingTimeSeconds = action.payload.currentDrawingTimeSeconds,
      state.currentRound = action.payload.currentRound,
      state.hiddenSecretWord = action.payload.hiddenSecretWord,
      state.drawingPlayerUsername = action.payload.drawingPlayerUsername,
      state.hostPlayerUsername = action.payload.hostPlayerUsername,
      state.isGameStarted = action.payload.isGameStarted,
      state.isTimerVisible = action.payload.isTimerVisible,
      state.correctGuessPlayerUsernames = action.payload.correctGuessPlayerUsernames
    },
    clearedGameState(state) {
      state.currentDrawingTimeSeconds = initialState.currentDrawingTimeSeconds,
      state.currentRound = initialState.currentRound,
      state.hiddenSecretWord = initialState.hiddenSecretWord,
      state.drawingPlayerUsername = initialState.drawingPlayerUsername,
      state.hostPlayerUsername = initialState.hostPlayerUsername,
      state.isGameStarted = initialState.isGameStarted,
      state.isTimerVisible = initialState.isTimerVisible,
      state.correctGuessPlayerUsernames = initialState.correctGuessPlayerUsernames
    }
  }
})

export const { updatedCurrentDrawingTimeSeconds, updatedCurrentRound, updatedHiddenSecretWord, updatedDrawingPlayerUsername,updatedHostPlayerUsername, updatedIsGameStarted, updatedIsTimerVisible, updatedCorrectGuessPlayerUsernames, updatedGameState, clearedGameState } = gameStateSlice.actions;
export default gameStateSlice.reducer;