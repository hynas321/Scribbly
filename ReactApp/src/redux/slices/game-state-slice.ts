import { createSlice, PayloadAction } from "@reduxjs/toolkit";

export interface GameState {
  currentDrawingTimeSeconds: number;
  currentRound: number;
  hiddenPromptText: string;
  promptCategory: string;
  promptKind: string;
  drawingPlayerUsername: string;
  hostPlayerUsername: string;
  isGameStarted: boolean;
  isTimerVisible: boolean;
  correctGuessPlayerUsernames: string[];
  canvasEpoch: number;
}

const initialState: GameState = {
  currentDrawingTimeSeconds: 50,
  currentRound: 1,
  hiddenPromptText: "? ? ?",
  promptCategory: "",
  promptKind: "word",
  drawingPlayerUsername: "",
  hostPlayerUsername: "",
  isGameStarted: false,
  isTimerVisible: false,
  correctGuessPlayerUsernames: [],
  canvasEpoch: 1,
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
    updatedHiddenPromptText(state, action: PayloadAction<string>) {
      state.hiddenPromptText = action.payload;
    },
    updatedPrompt(state, action: PayloadAction<{ text: string; category: string; kind: string }>) {
      state.hiddenPromptText = action.payload.text;
      state.promptCategory = action.payload.category;
      state.promptKind = action.payload.kind;
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
    updatedCanvasEpoch(state, action: PayloadAction<number>) {
      state.canvasEpoch = action.payload;
    },
    updatedGameState(_state, action: PayloadAction<GameState>) {
      return { ...action.payload };
    },
    clearedGameState() {
      return { ...initialState };
    },
  },
});

export const {
  updatedCurrentDrawingTimeSeconds,
  updatedCurrentRound,
  updatedHiddenPromptText,
  updatedPrompt,
  updatedDrawingPlayerUsername,
  updatedHostPlayerUsername,
  updatedIsGameStarted,
  updatedIsTimerVisible,
  updatedCorrectGuessPlayerUsernames,
  updatedCanvasEpoch,
  updatedGameState,
  clearedGameState,
} = gameStateSlice.actions;
export default gameStateSlice.reducer;
