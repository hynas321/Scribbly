import { useEffect, useState } from 'react';
import Button from '../Button';
import Form from '../Form';
import GameSettingsBoard from '../GameSettingsBoard';
import { useAppSelector } from '../../redux/hooks';
import EndpointHandler from "../../utils/EndpointHandler";
import config from '../../../config.json';
import Alert from '../Alert';
import { useNavigate } from 'react-router-dom';

function CreateGameMenu() {
  const minUsernameLength: number = 5;
  const endpointHandler = new EndpointHandler();
  const gameSettings = useAppSelector((state) => state.gameSettings);

  const navigate = useNavigate();
  const [hostUsername, setHostUsername] = useState("");
  const [activeButton, setActiveButton] = useState(false);
  const [alertText, setAlertText] = useState("");
  const [alertVisible, setAlertVisible] = useState(false);
  const [alertType, setAlertType] = useState("primary");

  const handleInputFormChange = (value: string) => {
    setHostUsername(value.trim());
  }

  const handleStartGameButtonClick = async () => {
    setActiveButton(false);

    const response = await endpointHandler.createGame(
      config.createGameServerEndpoint,
      hostUsername,
      gameSettings
    );

    setActiveButton(true);

    if (response.status != 201)  {
      setAlertVisible(true);
      setAlertText("Could not create a new game. Try Again.");
      setAlertType("danger");

      setTimeout(() => {
        setAlertVisible(false);
      }, 4000);

      return;
    }


    navigate(config.gameClientEndpoint);
  }

  useEffect(() => {
    if (hostUsername.length >= minUsernameLength) {
      setActiveButton(true);
    } 
    else {
      setActiveButton(false);
    }
  }, [hostUsername]);

  return (
    <div className="container">
      <div className="col-lg-4 col-sm-5 col-xs-6 mx-auto text-center">
        <Alert
            visible={alertVisible}
            text={alertText}
            type={alertType}
        />
        <Form
          placeholderValue="Enter username"
          smallTextValue={`Minimum username length ${minUsernameLength}`}
          onChange={handleInputFormChange}
        />
        <Button
          text="Create new game"
          type="success"
          active={activeButton}
          onClick={handleStartGameButtonClick}
        />
      </div>
      <div className="col-lg-5 col-md-7 col-sm-5 col-xs-5 mt-5 mx-auto">
        <GameSettingsBoard />
      </div>
    </div>
  );
}

export default CreateGameMenu;