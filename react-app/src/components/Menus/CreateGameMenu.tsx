import { useEffect, useState } from 'react';
import GreenButton from '../GreenButton';
import Form from '../Form';
import GameSettingsBoard from '../GameSettingsBoard';
import { useAppSelector } from '../../redux/hooks'
import EndpointHandler from "../../utils/EndpointHandler";
import config from '../../../config.json'

function CreateGameMenu() {
  const minUsernameLength: number = 5;
  const endpointHandler = new EndpointHandler();
  const gameSettings = useAppSelector((state) => state.gameSettings)
  
  const [hostUsername, setHostUsername] = useState("");
  const [activeButton, setActiveButton] = useState(false);

  const handleInputFormChange = (value: string) => {
    setHostUsername(value.trim());
  }

  const handleStartGameButtonClick = () => {
    endpointHandler.createGame(
      config.createGameEndpoint,
      hostUsername,
      gameSettings
    );
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
      <div className="col-lg-4 col-sm-5 col-xs-6 mx-auto mt-3 text-center">
        <h1 className="text-success">Scribbly</h1>
        <Form
          placeholderValue="Enter username"
          smallTextValue={`Minimum username length ${minUsernameLength}`}
          onChange={handleInputFormChange}
        />
        <GreenButton
          text="Create new game"
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

export default CreateGameMenu