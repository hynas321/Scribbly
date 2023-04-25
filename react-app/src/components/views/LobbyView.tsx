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
import ClipboardBar from '../bars/ClipboardBar';
import { Player } from '../../redux/slices/player-slice';
import { useContext } from "react";
import { LobbyHubContext } from '../../context/LobbyHubContext';
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

  const testLobbyHash = "TestLobbyHash"; //temporary
  const invitationUrl: string = "http://www.example.com"; //will be fetched from the server
  const isPlayerHost: boolean  = false; //will be fetched from the server

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
      await lobbyHub.invoke("JoinLobby", testLobbyHash, player.username);
    }

      const clearBeforeUnload = () => {
        lobbyHub.off("PlayerJoinedLobby");
        lobbyHub.off("PlayerLeftLobby");
        lobbyHub.send("LeaveLobby", testLobbyHash, player.username);
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
    <div className="container mb-3">
      <div className="row col-lg-6 col-sm-12 mx-auto text-center">
        <Alert
          visible={alertVisible}
          text={alertText}
          type={alertType}
        />
      </div>
      <div className="row">
        <div className="col-lg-4 col-sm-5 col-12 mx-auto mt-2 text-center order-lg-1 order-2 mb-3">
          <div className="col-lg-6">
            <PlayerList
              title={"Players in the lobby"}
              players={playerList}
              displayPoints={false}
              displayIndex={false}
            />
          </div>
        </div>
        <div className="col-lg-4 col-sm-10 col-12 mx-auto text-center order-lg-2 order-1">
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
        <div className="col-lg-4 order-lg-3 col-sm-7 col-12 order-3">
          <div className="col-lg-9 col-sm-12 col-12 float-end mb-3">
            <Chat 
              hubType={HubType.LOBBY}
              placeholderValue={"Enter your message"}
            />
          </div>
        </div>
        <div className="order-lg-4 order-4">
          <ClipboardBar invitationUrl={invitationUrl} />
        </div>
      </div>
    </div>
  );
}

export default LobbyView;