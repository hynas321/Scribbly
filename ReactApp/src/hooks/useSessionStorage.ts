import { useReducer } from "react";

export const useSessionStorage = () => {
  const [, forceUpdate] = useReducer(x => x + 1, 0);

  const username = sessionStorage.getItem("username") ?? "";
  const authorizationToken = sessionStorage.getItem("authorizationToken") ?? "";
  const authOToken = sessionStorage.getItem("authOToken") ?? "";
  const authOAccountId = sessionStorage.getItem("authOAccountId") ?? "";

  const setUsername = (value: string) => {
    sessionStorage.setItem("username", value);
    forceUpdate();
  };
  const setAuthorizationToken = (value: string) => {
    sessionStorage.setItem("authorizationToken", value);
    forceUpdate();
  };
  const setAuthOToken = (value: string) => {
    sessionStorage.setItem("authOToken", value);
    forceUpdate();
  };
  const setAuthOAccountId = (value: string) => {
    sessionStorage.setItem("authOAccountId", value);
    forceUpdate();
  };
  const clearAuthorizationToken = () => {
    sessionStorage.removeItem("authorizationToken");
    forceUpdate();
  };

  return {
    username,
    setUsername,
    authorizationToken,
    setAuthorizationToken,
    clearAuthorizationToken,
    authOToken,
    setAuthOToken,
    authOAccountId,
    setAuthOAccountId,
  };
};
