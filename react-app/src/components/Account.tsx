import { gapi } from "gapi-script";
import { useEffect, useState } from "react";
import GoogleLogin, { GoogleLogout } from "react-google-login";

function Account() {
  const clientId = "468363525055-d5ul5o6le0298njn348po0td776it110.apps.googleusercontent.com";

  const [isUserLoggedIn, setIsUserLoggedIn] = useState(false);
  const [givenName, setGivenName] = useState("");

  useEffect(() => {
    const start = () => {
      gapi.client.init({
        clientId: clientId,
        scope: ""
      });
    }

    gapi.load("client:auth2", start);
  }, []);

  const onSuccess = (res: any) => {
    setIsUserLoggedIn(true);
    setGivenName(res.profileObj.givenName);
  }

  const onLogoutSuccess = () => {
    setIsUserLoggedIn(false);
  }

  return (
    <div className="d-flex justify-content-end">
      <h6 className="mt-2">
        { isUserLoggedIn && `${givenName} (points: 12)` }
      </h6>
      <div className="mx-3">
        {
          isUserLoggedIn ?
          <GoogleLogout
            clientId={clientId}
            buttonText="Logout"
            onLogoutSuccess={onLogoutSuccess}
          /> :
          <GoogleLogin
            clientId={clientId}
            buttonText="Login"
            onSuccess={onSuccess}
            cookiePolicy={"single_host_origin"}
            isSignedIn={true}
          />
        }
      </div>
    </div>
  )
}

export default Account;