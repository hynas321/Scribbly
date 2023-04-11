import React from 'react'
import ReactDOM from 'react-dom/client'
import { Provider } from 'react-redux';
import { store } from './redux/store';
import 'bootstrap/dist/css/bootstrap.css'
import { RouterProvider, createBrowserRouter } from 'react-router-dom';
import CreateMatchView from './components/views/CreateMatchView';
import MatchView from './components/views/MatchView';
import Logo from './components/Logo';
import PageNotFound from './components/views/PageNotFound';
import config from '../config.json';

const router = createBrowserRouter([
  {
    path: config.createGameClientEndpoint,
    element: <CreateMatchView />,
  },
  {
    path: config.gameClientEndpoint,
    element: <MatchView />
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
