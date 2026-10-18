import { useEffect, useRef, useState } from "react";
import { loadGoogleIdentity } from "./googleIdentity";
import { useTranslation } from "../../i18n/I18nContext";

export default function GoogleSignInButton({
  onSignIn,
}: {
  onSignIn: (idToken: string) => Promise<void>;
}) {
  const { t } = useTranslation();
  const target = useRef<HTMLDivElement>(null);
  const callback = useRef(onSignIn);
  callback.current = onSignIn;
  const [failed, setFailed] = useState(false);
  const [attempt, setAttempt] = useState(0);
  const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID ?? "";

  useEffect(() => {
    if (!clientId) {
      return;
    }
    let active = true;
    setFailed(false);
    void loadGoogleIdentity()
      .then((google) => {
        if (!active || !target.current) {
          return;
        }
        google.initialize({
          client_id: clientId,
          callback: (response) => {
            if (active) {
              void callback.current(response.credential);
            }
          },
        });
        google.renderButton(target.current, { theme: "outline", size: "large" });
      })
      .catch(() => {
        if (active) {
          setFailed(true);
        }
      });
    return () => {
      active = false;
    };
  }, [clientId, attempt]);

  if (!clientId) {
    return <span className="text-muted">{t("account.signInUnavailable")}</span>;
  }
  return (
    <>
      <div ref={target} />
      {failed && (
        <button
          className="btn btn-outline-secondary"
          onClick={() => setAttempt((value) => value + 1)}
        >
          {t("account.retrySignIn")}
        </button>
      )}
    </>
  );
}
