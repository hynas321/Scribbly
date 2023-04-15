import 'bootstrap/dist/css/bootstrap.css'
import MainView from './views/MainView';
import { RouterProvider, createBrowserRouter } from 'react-router-dom';
import LobbyView from './views/LobbyView';
import GameView from './views/GameView';
import config from '../../config.json'
import PageNotFound from './views/PageNotFound';
import Logo from './Logo';
import { useAppSelector } from '../redux/hooks';

function App() {
    const router = createBrowserRouter([
      {
        path: config.mainClientEndpoint,
        element: <MainView />
      },
      {
        path: config.createGameClientEndpoint,
        element: <LobbyView />,
        children: [
          {
          path: '',
          element: <LobbyView />
          },
          {
          path: ':lobbyId',
          element: <LobbyView />
          }
        ]
      },
      {
        path: config.gameClientEndpoint,
        element: <GameView />,
        children: [
          {
          path: '',
          element: <GameView />
          },
          {
          path: ':gameId',
          element: <GameView />
          }
        ]
        
      },
      {
        path: "*",
        element: <PageNotFound/ >
      }
  ]);

  return (
    <>
      <Logo />
      <RouterProvider router={router} />
    </>
  )
}

export default App
