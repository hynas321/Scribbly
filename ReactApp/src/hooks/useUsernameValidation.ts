import { useSessionStorage } from "./useSessionStorage";

export const useUsernameValidation = (minLength: number, maxLength: number) => {
  const { username, setUsername } = useSessionStorage();
  return {
    username,
    setUsername,
    isValid: username.length >= minLength && username.length <= maxLength,
  };
};
