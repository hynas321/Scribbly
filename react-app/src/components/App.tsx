import 'bootstrap/dist/css/bootstrap.css'
import MainView from './views/MainView';
import { RouterProvider, createBrowserRouter } from 'react-router-dom';
import config from '../../config.json'
import PageNotFoundView from './views/PageNotFoundView';
import Header from './bars/Header';
import { Provider } from 'react-redux';
import { store } from '../redux/store';
import { AccountHubContext, ConnectionHubContext, LongRunningConnectionHubContext, connectionHub, longRunningConnectionHub, accountConnectionHub } from "../context/ConnectionHubContext";
import GameView from './views/GameView';
import JoinGameView from './views/JoinGameView';

function App() {
  const router = createBrowserRouter([
    {
      path: config.mainClientEndpoint,
      element: <MainView />
    },
    {
      path: `${config.gameClientEndpoint}/:gameHash`,
      element: <GameView />,
    },
    {
      path: `${config.joinGameClientEndpoint}/:gameHash`,
      element: <JoinGameView />
    },
    {
      path: "*",
      element: <PageNotFoundView/ >
    }
  ]);

return (
  <ConnectionHubContext.Provider value={connectionHub}>
    <LongRunningConnectionHubContext.Provider value={longRunningConnectionHub}>
      <AccountHubContext.Provider value={accountConnectionHub}>
        <Provider store={store}>
          <Header />
          <RouterProvider router={router}/>
        </Provider>
      </AccountHubContext.Provider>
    </LongRunningConnectionHubContext.Provider>
  </ConnectionHubContext.Provider>
  )
}

export default App
