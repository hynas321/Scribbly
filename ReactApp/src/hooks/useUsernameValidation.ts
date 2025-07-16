import { useEffect, useState } from "react";
import { useSessionStorage } from "./useSessionStorage";

export const useUsernameValidation = (minLength: number, maxLength: number) => {
  const { username, setUsername } = useSessionStorage();
  const [isValid, setIsValid] = useState(false);

  useEffect(() => {
    setIsValid(username.length >= minLength && username.length <= maxLength);
  }, [username, minLength, maxLength]);

  return { username, setUsername, isValid };
};
