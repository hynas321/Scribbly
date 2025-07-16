import { useState } from "react";
import { useGoogleLogout } from "react-google-login";
import api from "../http/Api";

export const useGoogleAccount = (clientId: string) => {
  const [givenName, setGivenName] = useState<string>("");
  const [score, setScore] = useState<number>(-1);
  const [authOToken, setAuthOToken] = useState<string>("");
  const [authOAccountId, setAuthOAccountId] = useState<string>("");
  const [isUserLoggedIn, setIsUserLoggedIn] = useState<boolean>(false);

  const setScoreState = async (googleId: string) => {
    let score = await api.fetchAccountScore(googleId);
    if (typeof score === "number") {
      setScore(score);
    }
  };

  const onSuccess = async (response: any, onHubStart: (googleId: string) => Promise<void>) => {
    await api.addAccountIfNotExists(response.profileObj, response.accessToken);
    await setScoreState(response.profileObj.googleId);
    setAuthOToken(response.accessToken);
    setGivenName(response.profileObj.givenName);
    setAuthOAccountId(response.profileObj.googleId);
    setIsUserLoggedIn(true);

    await onHubStart(response.profileObj.googleId);
  };

  const onLogoutSuccess = async (onHubEnd: (googleId: string) => Promise<void>) => {
    await onHubEnd(authOAccountId);
    setIsUserLoggedIn(false);
    setAuthOToken("");
    setGivenName("");
    setAuthOAccountId("");
  };

  const { signOut } = useGoogleLogout({
    clientId: clientId,
    onLogoutSuccess: () => onLogoutSuccess(async () => {}),
  });

  return {
    givenName,
    score,
    authOToken,
    authOAccountId,
    isUserLoggedIn,
    setScoreState,
    onSuccess,
    onLogoutSuccess,
    signOut,
    setScore,
    setIsUserLoggedIn,
    setAuthOToken,
    setGivenName,
    setAuthOAccountId,
  };
};
