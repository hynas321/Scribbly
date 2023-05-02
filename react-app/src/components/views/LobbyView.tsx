import { useContext, useEffect, useState } from 'react';
import Button from '../Button';
import GameSettingsBoard from '../GameSettingsBoard';
import { useAppSelector } from '../../redux/hooks';
import Alert from '../Alert';
import PlayerList from '../PlayerList';
import Chat from '../Chat';
import config from '../../../config.json';
import { BsPlayCircle, BsDoorOpen } from 'react-icons/bs';
import ClipboardBar from '../bars/ClipboardBar';
import { ConnectionHubContext } from '../../context/ConnectionHubContext';
import HubEvents from '../../hub/HubEvents';
import { updatedPlayerList } from '../../redux/slices/game-state-slice';
import { useDispatch } from 'react-redux';
import HttpRequestHandler from '../../utils/HttpRequestHandler';
import { HubConnectionState } from '@microsoft/signalr';
import { useNavigate } from 'react-router-dom';

function LobbyView() {
  const hub = useContext(ConnectionHubContext);
  const httpRequestHandler = new HttpRequestHandler();
  const player = useAppSelector((state) => state.player);
  const playerList = useAppSelector((state) => state.gameState.playerList);
  const dispatch = useDispatch();
  const navigate = useNavigate();

  let isPlayerHost: boolean = false;

  const [invitationUrl, setInvitationUrl] = useState(player.gameHash);
  const [activeButton, setActiveButton] = useState(true);
  const [alertText, setAlertText] = useState("");
  const [alertVisible, setAlertVisible] = useState(false);
  const [alertType, setAlertType] = useState("primary");

  const handleStartGameButtonClick = async () => {
    await hub.invoke(HubEvents.startGame, player.gameHash, player.username);
  }

  const handleLeaveGameButtonClick = async () => {
    const leaveGameBody = {
      gameHash: player.gameHash,
      token: player.token
    };

    await hub.invoke(HubEvents.leaveGame, leaveGameBody);
    navigate(config.mainClientEndpoint);
  }

  useEffect(() => {
    if (hub.getState() !== HubConnectionState.Connected) {
      return;
    }

    const checkIfPlayerIsHost = async () => {
      isPlayerHost = await httpRequestHandler.fetchPlayerIsHost(player.token, player.gameHash);
    }

    const setConnectionHub = async () => {

      await httpRequestHandler.fetchGameHash(player.token)
        .then((data) => {
          invitationUrl == `${config.gameClientEndpoint}/${data}` 
        });

      const getPlayerList = (playerListSerialized: string) => {
        const deserializedPlayerList = JSON.parse(playerListSerialized) as PlayerScore[];

        dispatch(updatedPlayerList(deserializedPlayerList));
      }
      
      hub.on(HubEvents.onPlayerJoinedGame, getPlayerList);
      hub.on(HubEvents.onPlayerLeftGame, getPlayerList);

      await hub.start();
    }

    const clearBeforeUnload = () => {
      hub.off(HubEvents.onPlayerJoinedGame);
      hub.off(HubEvents.onPlayerLeftGame);
      hub.send(HubEvents.leaveGame, player.gameHash, player.username);
    }

    setConnectionHub();
    checkIfPlayerIsHost();

    window.addEventListener("beforeunload", clearBeforeUnload);

    return () => {
      clearBeforeUnload();
      window.removeEventListener("beforeunload", clearBeforeUnload);
    }
  }, [hub.getState()]);

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
            <Button
              text="Leave the lobby"
              active={true}
              icon={<BsDoorOpen/>}
              type={"danger"}
              onClick={handleLeaveGameButtonClick}
            />
          <div className="mt-3">
            <GameSettingsBoard isPlayerHost={isPlayerHost} />
          </div>
        </div>
        <div className="col-lg-4 order-lg-3 col-sm-7 col-12 order-3">
          <div className="col-lg-9 col-sm-12 col-12 float-end mb-3">
            <Chat placeholderValue={"Enter your message"}/>
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