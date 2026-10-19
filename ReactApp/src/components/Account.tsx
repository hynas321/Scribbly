import { useAccount } from "../features/accounts/useAccount";
import GoogleSignInButton from "../features/accounts/GoogleSignInButton";
import { useTranslation } from "../i18n/I18nContext";

export default function Account() {
  const { profile, busy, signIn, signOut } = useAccount();
  const { t } = useTranslation();
  return (
    <div className="d-flex justify-content-end align-items-center gap-3" aria-busy={busy}>
      {profile ? (
        <>
          <span>
            {profile.name}{" "}
            <span className="badge rounded-pill bg-success">
              {t("common.scorePoints", { score: profile.score })}
            </span>
          </span>
          <button className="btn btn-outline-secondary" onClick={() => void signOut()}>
            {t("account.logout")}
          </button>
        </>
      ) : busy ? (
        <span role="status">{t("account.signingIn")}</span>
      ) : (
        <GoogleSignInButton onSignIn={signIn} />
      )}
    </div>
  );
}
