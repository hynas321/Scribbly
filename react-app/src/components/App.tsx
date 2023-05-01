import 'bootstrap/dist/css/bootstrap.css'
import MainView from './views/MainView';
import { RouterProvider, createBrowserRouter } from 'react-router-dom';
import config from '../../config.json'
import PageNotFound from './views/PageNotFound';
import Logo from './Logo';
import { Provider } from 'react-redux';
import { store } from '../redux/store';
import { ConnectionHubContext, connectionHub } from "../context/ConnectionHubContext";
import GameView from './views/GameView';
import LobbyView from './views/LobbyView';

function App() {
  const router = createBrowserRouter([
    {
      path: config.mainClientEndpoint,
      element: <MainView />
    },
    {
      path: config.gameClientEndpoint,
      element: <GameView />,
      children: [
        {
        path: ':gameId',
        element: <GameView />
        }
      ]
    },
    {
      path: config.lobbyClientEndpoint,
      element: <LobbyView />,
      children: [
        {
        path: ':gameId',
        element: <LobbyView />
        }
      ]
    },
    {
      path: "*",
      element: <PageNotFound/ >
    }
]);

return (
  <ConnectionHubContext.Provider value={connectionHub}>
    <Provider store={store}>
      <Logo />
      <RouterProvider router={router}/>
    </Provider>
  </ConnectionHubContext.Provider>
  )
}

export default App
