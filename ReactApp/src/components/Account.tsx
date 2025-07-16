import { useContext, useEffect, useState } from "react";
import { gapi } from "gapi-script";
import { AccountHubContext } from "../context/ConnectionHubContext";
import HubEvents from "../hub/HubMessages";
import api from "../http/Api";
import UrlHelper from "../utils/UrlHelper";
import GoogleLogin, { GoogleLogout } from "react-google-login";
import { useGoogleAccount } from "../hooks/useGoogleAccount";
import { useAccountHubEvents } from "../hooks/hub-events/useAccountHubEvents";

function Account() {
  const clientId = "468363525055-d5ul5o6le0298njn348po0td776it110.apps.googleusercontent.com";
  const accountHub = useContext(AccountHubContext);

  const {
    givenName, score, authOToken, authOAccountId, isUserLoggedIn,
    setScore, onSuccess, onLogoutSuccess
  } = useGoogleAccount(clientId);

  const [isScoreToBeUpdated, setIsScoreToBeUpdated] = useState(false);
  const [gameHash, setGameHash] = useState("");

  useEffect(() => {
    setGameHash(UrlHelper.getGameHash(window.location.href));

    const start = () => {
      gapi.client.init({ clientId: clientId, scope: "" });
    };
    gapi.load("client:auth2", start);

    return () => {
      accountHub.off(HubEvents.onUpdateAccountScore);
    };
  }, []);

  useAccountHubEvents(accountHub, authOAccountId, isUserLoggedIn, setIsScoreToBeUpdated);

  useEffect(() => {
    const updateScore = async () => {
      if (isScoreToBeUpdated) {
        const updatedScore = await api.updateAccountScore(gameHash, "", authOToken);
        if (typeof updatedScore === "number") {
          setScore(updatedScore);
        }
        setIsScoreToBeUpdated(false);
      }
    };
    updateScore();
  }, [isScoreToBeUpdated]);

  const handleSuccess = async (response: any) => {
    await onSuccess(response, async (googleId: string) => {
      await accountHub.start();
      await accountHub.send(HubEvents.createSession, googleId);
    });
  };

  const handleLogout = async () => {
    await onLogoutSuccess(async (googleId: string) => {
      await accountHub.send(HubEvents.endSession, googleId);
      await accountHub.stop();
    });
  };

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
          <GoogleLogout clientId={clientId} buttonText="Logout" onLogoutSuccess={handleLogout} />
        ) : (
          <GoogleLogin
            clientId={clientId}
            buttonText="Login"
            onSuccess={handleSuccess}
            cookiePolicy={"single_host_origin"}
            isSignedIn={true}
          />
        )}
      </div>
    </div>
  );
}

export default Account;
