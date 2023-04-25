import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Player } from './player-slice';

export interface GameState {
  currentDrawingTimeSeconds: number;
  currentRound: number;
  players: Player[];
  onlinePlayersTokens: string[]
};

const initialState: GameState = {
  currentDrawingTimeSeconds: 50,
  currentRound: 1,
  players: [],
  onlinePlayersTokens: []
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
    updatedPlayers(state, action: PayloadAction<Player[]>) {
      state.players = action.payload;
    },
    updatedOnlinePlayersUsernames(state, action: PayloadAction<string[]>) {
      state.onlinePlayersTokens = action.payload;
    }
  }
})

export const { updatedCurrentDrawingTimeSeconds, updatedCurrentRound, updatedPlayers, updatedOnlinePlayersUsernames } = gameStateSlice.actions;
export default gameStateSlice.reducer;