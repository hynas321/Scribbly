import { useCallback, useEffect, useRef, useState } from "react";
import { toast } from "react-toastify";
import api, { AccountProfile } from "../../http/Api";
import { ToastNotificationEnum } from "../../enums/ToastNotificationEnum";
import { useSessionStorage } from "../../hooks/useSessionStorage";
import { useTranslation } from "../../i18n/I18nContext";

const notifyError = (message: string) =>
  toast.error(message, { containerId: ToastNotificationEnum.Main });

export function useAccount() {
  const { t } = useTranslation();
  const [profile, setProfile] = useState<AccountProfile | null>(null);
  const [busy, setBusy] = useState(true);
  const loginPending = useRef(false);
  const { completedGameHash, completedRoomAccessToken, clearCompletedGame } = useSessionStorage();

  useEffect(() => {
    let active = true;
    void api
      .fetchAccountSession()
      .then((restored) => {
        if (active) {
          setProfile(restored);
        }
      })
      .catch(() => undefined)
      .finally(() => {
        if (active) {
          setBusy(false);
        }
      });
    return () => {
      active = false;
    };
  }, []);

  useEffect(() => {
    if (!profile || !completedGameHash || !completedRoomAccessToken) {
      return;
    }
    let active = true;
    void api
      .updateAccountScore(completedGameHash, completedRoomAccessToken)
      .then((score) => {
        if (!active) {
          return;
        }
        setProfile((current) => (current ? { ...current, score } : current));
        clearCompletedGame();
      })
      .catch(() => undefined);
    return () => {
      active = false;
    };
  }, [profile, completedGameHash, completedRoomAccessToken, clearCompletedGame]);

  const signIn = useCallback(
    async (idToken: string) => {
      if (loginPending.current) {
        return;
      }
      loginPending.current = true;
      setBusy(true);
      try {
        setProfile(await api.signIn(idToken));
      } catch {
        notifyError(t("account.signInError"));
      } finally {
        loginPending.current = false;
        setBusy(false);
      }
    },
    [t],
  );

  const signOut = useCallback(async () => {
    setProfile(null);
    window.google?.accounts.id.disableAutoSelect();
    try {
      await api.signOut();
    } catch {
      notifyError(t("account.signOutError"));
    }
  }, [t]);

  return { profile: profile ?? undefined, busy, signIn, signOut };
}
