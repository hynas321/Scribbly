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
import { Player } from '../../redux/slices/player-slice';
import { useContext } from "react";
import { LobbyHubContext } from '../../Context/LobbyHubContext';
import { HubType } from '../../enums/HubType';

function LobbyView() {
  const lobbyHub = useContext(LobbyHubContext);
  const player = useAppSelector((state) => state.player);
  const navigate = useNavigate();

  const [activeButton, setActiveButton] = useState(true);
  const [alertText, setAlertText] = useState("");
  const [alertVisible, setAlertVisible] = useState(false);
  const [alertType, setAlertType] = useState("primary");
  const [playerList, setPlayerList] = useState<Player[]>([]);

  const testLobbyUrl = "TestLobbyUrl"; //temporary
  const invitationUrl: string = "http://www.example.com"; //will be fetched from the server
  const isPlayerHost: boolean = useAppSelector((state) => state.player.host);

  const handleStartGameButtonClick = async () => {

    navigate(config.gameClientEndpoint);
  }

  useEffect(() => {

    const setLobbyHub = async () => {

      const getPlayerList = (playerListSerialized: any) => {
        const playerList = JSON.parse(playerListSerialized) as Player[];

        setPlayerList(playerList);
      }
      
      lobbyHub.on("PlayerJoinedLobby", getPlayerList);
      lobbyHub.on("PlayerLeftLobby", getPlayerList);

      await lobbyHub.start();
      await lobbyHub.invoke("JoinLobby", testLobbyUrl, player.username);
    }

      const clearBeforeUnload = () => {
        lobbyHub.off("PlayerJoinedLobby");
        lobbyHub.off("PlayerLeftLobby");
        lobbyHub.send("LeaveLobby", testLobbyUrl, player.username);
        lobbyHub.stop();
      }

    setLobbyHub();

    window.addEventListener("beforeunload", clearBeforeUnload);

    return () => {
      clearBeforeUnload();
      window.removeEventListener("beforeunload", clearBeforeUnload);
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
            players={playerList}
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
            <GameSettingsBoard
              hub={lobbyHub}
              isPlayerHost={isPlayerHost}
            />
          </div>
        </div>
        <div className="col-1"/>
        <div className="col-3">
          <Chat 
            hubType={HubType.LOBBY}
            placeholderValue={"Enter your message"}
          />
        </div>
        <div>
          <ClipboardBar invitationUrl={invitationUrl} />
        </div>
      </div>
    </div>
  );
}

export default LobbyView;