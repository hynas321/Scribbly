import { useState, useEffect } from "react";
import { SessionStorageService } from "../classes/SessionStorageService";

export const useSessionStorageUsername = (): [
  string,
  React.Dispatch<React.SetStateAction<string>>
] => {
  const sessionStorageService = SessionStorageService.getInstance();
  const [username, setUsernameState] = useState(sessionStorageService.getUsername());

  const setUsername = (newUsername: React.SetStateAction<string>) => {
    const newValue = typeof newUsername === "function" ? newUsername(username) : newUsername;

    sessionStorageService.setUsername(newValue);
    setUsernameState(newValue);
  };

  useEffect(() => {
    const handleStorageChange = () => {
      setUsernameState(sessionStorageService.getUsername());
    };

    window.addEventListener("storage", handleStorageChange);
    return () => {
      window.removeEventListener("storage", handleStorageChange);
    };
  }, [sessionStorageService]);

  return [username, setUsername];
};
