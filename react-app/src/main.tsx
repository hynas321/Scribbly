import React from 'react'
import ReactDOM from 'react-dom/client'
import { Provider } from 'react-redux';
import { store } from './redux/store';
import 'bootstrap/dist/css/bootstrap.css'
import { RouterProvider, createBrowserRouter } from 'react-router-dom';
import CreateGameView from './components/views/LobbyView';
import GameView from './components/views/GameView';
import Logo from './components/Logo';
import PageNotFound from './components/views/PageNotFound';
import config from '../config.json';
import MainView from './components/views/MainView';

const router = createBrowserRouter([
  {
    path: config.mainClientEndpoint,
    element: <MainView />
  },
  {
    path: config.createGameClientEndpoint,
    element: <CreateGameView />,
  },
  {
    path: config.gameClientEndpoint,
    element: <GameView />
  },
  {
    path: "*",
    element: <PageNotFound/ >
  }
]);

ReactDOM.createRoot(document.getElementById('root') as HTMLElement).render(
  <React.StrictMode>
    <Provider store={store}>
      <Logo />
      <RouterProvider router={router} />
    </Provider>
  </React.StrictMode>,
)
