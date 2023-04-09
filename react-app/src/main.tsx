import React from 'react'
import ReactDOM from 'react-dom/client'
import { Provider } from 'react-redux';
import { store } from './redux/store';
import 'bootstrap/dist/css/bootstrap.css'
import { RouterProvider, createBrowserRouter } from 'react-router-dom';
import CreateGameMenu from './components/views/CreateGameMenu';
import GameMenu from './components/views/GameMenu';
import Logo from './components/Logo';
import PageNotFound from './components/views/PageNotFound';
import config from '../config.json';

const router = createBrowserRouter([
  {
    path: config.createGameClientEndpoint,
    element: <CreateGameMenu />,
  },
  {
    path: config.gameClientEndpoint,
    element: <GameMenu />
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
