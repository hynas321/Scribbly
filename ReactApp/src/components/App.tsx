import "bootstrap/dist/css/bootstrap.css";
import MainView from "./views/MainView";
import { RouterProvider, createBrowserRouter } from "react-router-dom";
import config from "../../config.json";
import PageNotFoundView from "./views/PageNotFoundView";
import Header from "./bars/Header";
import { Provider } from "react-redux";
import { store } from "../redux/store";
import {
  AccountHubContext,
  ConnectionHubContext,
  LongRunningConnectionHubContext,
  connectionHub,
  longRunningConnectionHub,
  accountConnectionHub,
} from "../context/ConnectionHubContext";
import GameView from "./views/GameView";
import JoinGameView from "./views/JoinGameView";
import { ToastContainer } from "react-toastify";
import { ToastNotificationEnum } from "../enums/ToastNotificationEnum";
import "react-toastify/dist/ReactToastify.css";

function App() {
  const router = createBrowserRouter([
    {
      path: config.mainClientEndpoint,
      element: <MainView />,
    },
    {
      path: `${config.gameClientEndpoint}/:gameHash`,
      element: <GameView />,
    },
    {
      path: `${config.joinGameClientEndpoint}/:gameHash`,
      element: <JoinGameView />,
    },
    {
      path: "*",
      element: <PageNotFoundView />,
    },
  ]);

  return (
    <ConnectionHubContext.Provider value={connectionHub}>
      <LongRunningConnectionHubContext.Provider value={longRunningConnectionHub}>
        <AccountHubContext.Provider value={accountConnectionHub}>
          <Provider store={store}>
            <Header />
            <ToastContainer
              containerId={ToastNotificationEnum.Main}
              position="top-left"
              autoClose={3000}
              closeOnClick
              draggable
              pauseOnHover={false}
              theme="light"
              style={{ opacity: 0.9 }}
            />
            <RouterProvider router={router} />
          </Provider>
        </AccountHubContext.Provider>
      </LongRunningConnectionHubContext.Provider>
    </ConnectionHubContext.Provider>
  );
}

export default App;
