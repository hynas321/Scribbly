import "bootstrap/dist/css/bootstrap.css";
import "react-toastify/dist/ReactToastify.css";
import { lazy, Suspense } from "react";
import { RouterProvider, createBrowserRouter } from "react-router-dom";
import { Provider } from "react-redux";
import { ToastContainer } from "react-toastify";
import config from "../../config.json";
import PageNotFoundView from "./views/PageNotFoundView";
import Header from "./bars/Header";
import { store } from "../redux/store";
import { ConnectionHubContext, connectionHub } from "../context/ConnectionHubContext";
import { ToastNotificationEnum } from "../enums/ToastNotificationEnum";
import { I18nProvider, useTranslation } from "../i18n/I18nContext";

const MainView = lazy(() => import("./views/MainView"));
const GameView = lazy(() => import("./views/game-and-lobby/GameView"));
const JoinGameView = lazy(() => import("./views/JoinGameView"));
const GameResultsView = lazy(() => import("./views/game-and-lobby/GameResultsView"));

function RouteError() {
  const { t } = useTranslation();
  return (
    <div className="container py-4" role="alert">
      <p>{t("routeError.message")}</p>
      <a href={config.mainClientEndpoint}>{t("routeError.returnLink")}</a>
    </div>
  );
}

function LoadingFallback() {
  const { t } = useTranslation();
  return (
    <p className="text-center py-4" role="status">
      {t("app.loading")}
    </p>
  );
}

const router = createBrowserRouter([
  { path: config.mainClientEndpoint, errorElement: <RouteError />, element: <MainView /> },
  {
    path: `${config.gameClientEndpoint}/:gameHash/results`,
    errorElement: <RouteError />,
    element: <GameResultsView />,
  },
  {
    path: `${config.gameClientEndpoint}/:gameHash`,
    errorElement: <RouteError />,
    element: <GameView />,
  },
  {
    path: `${config.joinGameClientEndpoint}/:gameHash`,
    errorElement: <RouteError />,
    element: <JoinGameView />,
  },
  { path: "*", errorElement: <RouteError />, element: <PageNotFoundView /> },
]);

function App() {
  return (
    <I18nProvider>
      <Provider store={store}>
        <ConnectionHubContext.Provider value={connectionHub}>
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
          <Suspense fallback={<LoadingFallback />}>
            <RouterProvider router={router} />
          </Suspense>
        </ConnectionHubContext.Provider>
      </Provider>
    </I18nProvider>
  );
}

export default App;
