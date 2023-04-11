import { configureStore } from '@reduxjs/toolkit';
import gameSettingsReducer from './slices/game-settings-slice';
import playerReducer from './slices/player-slice';

export const store = configureStore({
  reducer: {
    gameSettings: gameSettingsReducer,
    player: playerReducer
  }
})

export type AppDispatch = typeof store.dispatch;
export type RootState = ReturnType<typeof store.getState>;