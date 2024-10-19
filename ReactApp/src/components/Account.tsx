import { gapi } from "gapi-script";
import { useContext, useEffect, useState } from "react";
import GoogleLogin, { GoogleLogout } from "react-google-login";
import HttpRequestHandler from "../http/HttpRequestHandler";
import { AccountHubContext } from "../context/ConnectionHubContext";
import HubEvents from "../hub/HubMessages";
import * as signalR from "@microsoft/signalr";
import { useGoogleLogout } from "react-google-login";
import UrlHelper from "../utils/UrlHelper";
import { SessionStorageService } from "../classes/SessionStorageService";

function Account() {
  const httpRequestHandler = new HttpRequestHandler();
  const clientId = "468363525055-d5ul5o6le0298njn348po0td776it110.apps.googleusercontent.com";

  const accountHub = useContext(AccountHubContext);

  const [gameHash, setGameHash] = useState<string>("");
  const [givenName, setGivenName] = useState<string>("");
  const [score, setScore] = useState<number>(-1);
  const [isUserLoggedIn, setIsUserLoggedIn] = useState<boolean>(false);
  const [isScoreToBeUpdated, setIsScoreToBeUpdated] = useState<boolean>(false);

  const sessionStorageService = SessionStorageService.getInstance();

  useEffect(() => {
    setGameHash(UrlHelper.getGameHash(window.location.href));

    const start = () => {
      gapi.client.init({
        clientId: clientId,
        scope: "",
      });
    };

    gapi.load("client:auth2", start);

    return () => {
      accountHub.off(HubEvents.onUpdateAccountScore);
    };
  }, []);

  useEffect(() => {
    if (accountHub.getState() !== signalR.HubConnectionState.Connected) {
      return;
    }

    accountHub.on(HubEvents.onUpdateAccountScore, async (gameHash: string) => {
      if (!isUserLoggedIn) {
        return;
      }

      if (gameHash !== UrlHelper.getGameHash(window.location.href)) {
        return;
      }

      setGameHash(UrlHelper.getGameHash(window.location.href));
      setIsScoreToBeUpdated(true);
    });

    accountHub.on(HubEvents.onSessionEnded, async () => {
      const currentAccountId = sessionStorageService.getAuthOAccountId();

      setIsUserLoggedIn(false);
      sessionStorageService.setAuthOToken("");
      setGivenName("");
      signOut();

      await accountHub.send(HubEvents.endSession, currentAccountId);
      await accountHub.stop();
    });
  }, [accountHub.getState()]);

  useEffect(() => {
    const updateScore = async () => {
      if (isScoreToBeUpdated) {
        const updatedScore = await httpRequestHandler.updateAccountScore(
          gameHash,
          sessionStorageService.getAuthorizationToken(),
          sessionStorageService.getAuthOToken()
        );

        if (typeof updatedScore == "number") {
          setScore(updatedScore);
        }

        setIsScoreToBeUpdated(false);
      }
    };

    updateScore();
  }, [isScoreToBeUpdated]);

  const onSuccess = async (response: any) => {
    await httpRequestHandler.addAccountIfNotExists(response.profileObj, response.accessToken);

    const setScoreState = async () => {
      let score = await httpRequestHandler.fetchAccountScore(response.profileObj.googleId);

      if (typeof score != "number") {
        return;
      }

      setScore(score);
    };

    await setScoreState();
    sessionStorageService.setAuthOToken(response.accessToken);
    setGivenName(response.profileObj.givenName);
    sessionStorageService.setAuthOAccountId(response.profileObj.googleId);
    setIsUserLoggedIn(true);

    await accountHub.start();
    await accountHub.send(HubEvents.createSession, response.profileObj.googleId);
  };

  const onLogoutSuccess = async () => {
    await accountHub.send(HubEvents.endSession, sessionStorageService.getAuthOAccountId());
    await accountHub.stop();

    setIsUserLoggedIn(false);
    sessionStorageService.setAuthOToken("");
    setGivenName("");
    sessionStorageService.setAuthOAccountId("");
  };

  const { signOut } = useGoogleLogout({
    clientId: sessionStorageService.getAuthOAccountId(),
    onLogoutSuccess: onLogoutSuccess,
  });

  return (
    <div className="d-flex justify-content-end">
      <h6 className="mt-2">
        {isUserLoggedIn && `${givenName} `}
        {isUserLoggedIn && (
          <span className="badge rounded-pill bg-success mt-2">{score} points</span>
        )}
      </h6>
      <div className="mx-3">
        {isUserLoggedIn ? (
          <GoogleLogout clientId={clientId} buttonText="Logout" onLogoutSuccess={onLogoutSuccess} />
        ) : (
          <GoogleLogin
            clientId={clientId}
            buttonText="Login"
            onSuccess={onSuccess}
            cookiePolicy={"single_host_origin"}
            isSignedIn={true}
          />
        )}
      </div>
    </div>
  );
}

export default Account;
