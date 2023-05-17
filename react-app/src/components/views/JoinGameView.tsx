import useLocalStorageState from "use-local-storage-state";
import InputForm from "../InputForm";
import Button from "../Button";
import { useEffect, useState } from "react";
import HttpRequestHandler from "../../http/HttpRequestHandler";
import { useNavigate } from "react-router-dom";
import { useDispatch } from "react-redux";
import { updatedAlert, updatedVisible } from "../../redux/slices/alert-slice";
import { updatedUsername } from "../../redux/slices/player-score-slice";
import UrlHelper from "../../utils/VerificationHelper";
import config from '../../../config.json';

function JoinGameView() { 
  const httpRequestHandler = new HttpRequestHandler();
  const minUsernameLength: number = 1;
  const maxUsernameLength: number = 18;

  const navigate = useNavigate();
  const dispatch = useDispatch();

  const [gameHash, setGameHash] = useState<string>("");
  const [isJoinGameButtonActive, setIsJoinGameButtonActive] = useState<boolean>(false);

  const [username, setUsername] = useLocalStorageState("username", { defaultValue: ""});

  const handleInputFormChange = (value: string) => {
    setUsername(value.trim());
  }

  const handleJoinGameButtonClick = async () => {
    if (username.length < minUsernameLength) {
      return;
    }

    const checkIfGameExists = async () => {
      try {
        const data = await httpRequestHandler.checkIfGameExists(gameHash);
    
        if (typeof data != "boolean") {
          displayAlert("Unexpected error, try again", "danger");
          return;
        }
    
        if (data === true) {
          dispatch(updatedUsername(username));
          navigate(`${config.gameClientEndpoint}/${gameHash}`);
        }
        else {
          displayAlert("Game does not exist", "danger");
        }
      
      }
      catch (error) {
        displayAlert("Unexpected error, try again", "danger");
      }
    };

    await checkIfGameExists();
  }

  useEffect(() => {
    setGameHash(UrlHelper.getGameHash(window.location.href));
  }, []);

  useEffect(() => {
    if (username.length < minUsernameLength ||
        username.length > maxUsernameLength) {
      setIsJoinGameButtonActive(false);
    } 
    else {
      setIsJoinGameButtonActive(true);
    }
  }, [username]);

  const displayAlert = (message: string, type: string) => {
    dispatch(updatedAlert({
      text: message,
      visible: true,
      type: type
    }));

    setTimeout(() => {
      dispatch(updatedVisible(false));
    }, 3000);
  }
  
  return (
    <div className="container">
      <div className="col-lg-4 col-sm-7 col-xs-6 mx-auto text-center">
        <InputForm
          defaultValue={username}
          placeholderValue="Enter username"
          smallTextValue={`Allowed username length ${minUsernameLength}-${maxUsernameLength}`}
          onChange={handleInputFormChange}
        />
        <Button
          text="Join the game"
          active={isJoinGameButtonActive}
          onClick={handleJoinGameButtonClick}
        />
      </div>
    </div>
  )
}

export default JoinGameView;