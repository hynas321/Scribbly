import { gapi } from "gapi-script";
import { useContext, useEffect, useState } from "react";
import GoogleLogin, { GoogleLogout } from "react-google-login";
import HttpRequestHandler from "../http/HttpRequestHandler";
import useLocalStorageState from "use-local-storage-state";
import { ConnectionHubContext } from "../context/ConnectionHubContext";
import HubEvents from "../hub/HubEvents";
import * as signalR from '@microsoft/signalr';

function Account() {
  const httpRequestHandler = new HttpRequestHandler();
  const clientId = "468363525055-d5ul5o6le0298njn348po0td776it110.apps.googleusercontent.com";

  const hub = useContext(ConnectionHubContext);

  const [givenName, setGivenName] = useState<string>("");
  const [score, setScore] = useState<number>(-1);
  const [isUserLoggedIn, setIsUserLoggedIn] = useState<boolean>(false);

  const [token, setToken] = useLocalStorageState("token", { defaultValue: "" });
  const [oAuthToken, setOAuthToken] = useLocalStorageState("oAuthToken", { defaultValue: ""});

  useEffect(() => {
    const start = () => {
      gapi.client.init({
        clientId: clientId,
        scope: ""
      });
    }

    gapi.load("client:auth2", start);
  }, []);

  useEffect(() => {
    if (hub.getState() !== signalR.HubConnectionState.Connected) {
      return;
    }

    hub.on(HubEvents.onUpdateAccountScore, async () => {
      if (!isUserLoggedIn) {
        return
      }

      const updatedScore = await httpRequestHandler.updateAccountScore(token, oAuthToken);

      if (typeof updatedScore == "number") {
        setScore(updatedScore);
      }
    });

  }, [hub.getState()]);

  const onSuccess = async (res: any) => {
    await httpRequestHandler.addAccountIfNotExists(res.profileObj, res.accessToken);

    const setScoreState = async () => {
      let score = await httpRequestHandler.fetchAccountScore(res.profileObj.googleId);

      if (typeof score != "number") {
        return;
      }

      setScore(score);
    }

    await setScoreState();
    setOAuthToken(res.accessToken);
    setGivenName(res.profileObj.givenName);
    setIsUserLoggedIn(true);
  }

  const onLogoutSuccess = () => {
    setIsUserLoggedIn(false);
    setOAuthToken("");
    setGivenName("");
  }

  return (
    <div className="d-flex justify-content-end">
      <h6 className="mt-2">
        { isUserLoggedIn && `${givenName} ` }
        { isUserLoggedIn && <span className="badge rounded-pill bg-success mt-2">{score} points</span>}
      </h6>
      <div className="mx-3">
        {
          isUserLoggedIn ?
          <GoogleLogout
            clientId={clientId}
            buttonText="Logout"
            onLogoutSuccess={onLogoutSuccess}
          /> :
          <GoogleLogin
            clientId={clientId}
            buttonText="Login"
            onSuccess={onSuccess}
            cookiePolicy={"single_host_origin"}
            isSignedIn={true}
          />
        }
      </div>
    </div>
  )
}

export default Account;