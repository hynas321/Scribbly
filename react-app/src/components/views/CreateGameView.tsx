import {useState } from 'react';
import Button from '../Button';
import GameSettingsBoard from '../GameSettingsBoard';
import { useAppDispatch, useAppSelector } from '../../redux/hooks';
import EndpointHandler from "../../utils/EndpointHandler";
import config from '../../../config.json';
import Alert from '../Alert';
import { Link, useNavigate } from 'react-router-dom';
import PlayerList from '../PlayerList';
import Chat from '../Chat';

function CreateGameView() {
  const player = useAppSelector((state) => state.player);
  const navigate = useNavigate();
  const [activeButton, setActiveButton] = useState(true);
  const [alertText, setAlertText] = useState("");
  const [alertVisible, setAlertVisible] = useState(false);
  const [alertType, setAlertType] = useState("primary");

  const handleStartGameButtonClick = async () => {
    /*
    setActiveButton(false);
    
    const response = await endpointHandler.createGame(
      config.createGameServerEndpoint,
      player.username,
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
    */

    navigate(config.gameClientEndpoint);
  }

  return (
    <div className="container">
      <div className="row col-6 mx-auto text-center">
        <Alert
          visible={alertVisible}
          text={alertText}
          type={alertType}
        />
      </div>
      <div className="row">
        <div className="col-2 mx-auto text-center">
        <PlayerList
          title={"Players in the lobby"}
          players={[player, player, player, player, player, player, player, player]}
          displayPoints={false}
        />
        </div>
        <div className="col-2"/>
        <div className="col-4 mx-auto text-center">
          <h5>Host username: {player.username}</h5>
          <Button
            text="Start the game"
            type="success"
            active={activeButton}
            onClick={handleStartGameButtonClick}
          />
          <Link to={config.mainClientEndpoint}>
            <Button
              text={"Leave the lobby"}
              active={true}
              type={"danger"}
            />
          </Link>
          <div className="mt-3">
            <GameSettingsBoard />
          </div>
        </div>
        <div className="col-1"/>
        <div className="col-3">
          <Chat placeholderValue={"Enter your message"} />
        </div>
      </div>
    </div>
  );
}

export default CreateGameView;