import { useEffect, useState } from 'react';
import Button from '../Button';
import GameSettingsBoard from '../GameSettingsBoard';
import { useAppSelector } from '../../redux/hooks';
import config from '../../../config.json';
import Alert from '../Alert';
import { Link, useNavigate } from 'react-router-dom';
import PlayerList from '../PlayerList';
import Chat from '../Chat';
import { BsPlayCircle, BsDoorOpen } from 'react-icons/bs';
import ClipboardBar from '../ClipboardBar';
import * as signalR from '@microsoft/signalr'
import LobbyHub from '../Hubs/LobbyHub';

function LobbyView() {
  const lobbyHub: LobbyHub = new LobbyHub(`${config.httpServerUrl}${config.hubLobbyEndpoint}`);
  const player = useAppSelector((state) => state.player);
  const navigate = useNavigate();

  const [activeButton, setActiveButton] = useState(true);
  const [alertText, setAlertText] = useState("");
  const [alertVisible, setAlertVisible] = useState(false);
  const [alertType, setAlertType] = useState("primary");
  const [lobbyHubConnection, setLobbyHubConnection] = useState<signalR.HubConnection | null>(null);

  const invitationUrl: string = "http://www.example.com"; //will be fetched from the server
  const isPlayerHost: boolean = useAppSelector((state) => state.player.host);

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

  useEffect(() => {
    lobbyHub.start();

    return() => {
      lobbyHub.stop();
    }
  }, []);

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
            displayIndex={false}
          />
        </div>
        <div className="col-2"/>
        <div className="col-4 mx-auto text-center">
          <h5>Your username: {player.username}</h5>
          { isPlayerHost && 
            <Button
              text="Start the game"
              type="success"
              active={activeButton}
              icon={<BsPlayCircle/>}
              onClick={handleStartGameButtonClick}
            />
          }
          { !isPlayerHost && <h4 className="mt-3">Waiting for the host to start the game</h4> }
          <Link to={config.mainClientEndpoint}>
            <Button
              text="Leave the lobby"
              active={true}
              icon={<BsDoorOpen/>}
              type={"danger"}
            />
          </Link>
          <div className="mt-3">
            <GameSettingsBoard isPlayerHost={isPlayerHost} />
          </div>
        </div>
        <div className="col-1"/>
        <div className="col-3">
          <Chat placeholderValue={"Enter your message"} />
        </div>
        <div>
          <ClipboardBar invitationUrl={invitationUrl} />
        </div>
      </div>
    </div>
  );
}

export default LobbyView;