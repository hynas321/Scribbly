import 'bootstrap/dist/css/bootstrap.css'
import MainView from './views/MainView';
import { RouterProvider, createBrowserRouter } from 'react-router-dom';
import config from '../../config.json'
import PageNotFound from './views/PageNotFound';
import LogoAndAccountBar from './bars/LogoAndAccountBar';
import { Provider } from 'react-redux';
import { store } from '../redux/store';
import { ConnectionHubContext, LongRunningConnectionHubContext, connectionHub, longRunningConnectionHub } from "../context/ConnectionHubContext";
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
      <Provider store={store}>
        <LogoAndAccountBar />
        <RouterProvider router={router}/>
      </Provider>
    </LongRunningConnectionHubContext.Provider>
  </ConnectionHubContext.Provider>
  )
}

export default App
