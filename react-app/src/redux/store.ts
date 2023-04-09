import { configureStore } from '@reduxjs/toolkit';
import gameSettingsReducer from './slices/game-settings-slice';

export const store = configureStore({
  reducer: {
    gameSettings: gameSettingsReducer
  }
})

export type AppDispatch = typeof store.dispatch;
export type RootState = ReturnType<typeof store.getState>;