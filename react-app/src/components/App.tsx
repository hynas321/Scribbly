import 'bootstrap/dist/css/bootstrap.css'
import MainView from './views/MainView';
import { RouterProvider, createBrowserRouter } from 'react-router-dom';
import config from '../../config.json'
import PageNotFound from './views/PageNotFound';
import LogoAndAccountBar from './bars/LogoAndAccountBar';
import { Provider } from 'react-redux';
import { store } from '../redux/store';
import { AccountHubContext, ConnectionHubContext, LongRunningConnectionHubContext, connectionHub, longRunningConnectionHub, accountConnectionHub } from "../context/ConnectionHubContext";
import GameView from './views/GameView';

function App() {
  const router = createBrowserRouter([
    {
      path: config.mainClientEndpoint,
      element: <MainView />
    },
    {
      path: config.gameClientEndpoint,
      element: <GameView />,
    },
    {
      path: "*",
      element: <PageNotFound/ >
    }
]);

return (
  <ConnectionHubContext.Provider value={connectionHub}>
    <LongRunningConnectionHubContext.Provider value={longRunningConnectionHub}>
      <AccountHubContext.Provider value={accountConnectionHub}>
        <Provider store={store}>
          <LogoAndAccountBar />
          <RouterProvider router={router}/>
        </Provider>
      </AccountHubContext.Provider>
    </LongRunningConnectionHubContext.Provider>
  </ConnectionHubContext.Provider>
  )
}

export default App
