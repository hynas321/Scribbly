import { useContext, useEffect, useRef, useState } from 'react';
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
import { useDispatch } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import HttpRequestHandler from '../../http/HttpRequestHandler';
import { updatedAlert, updatedVisible } from '../../redux/slices/alert-slice';
import { addedPlayerScore, updatedPlayerList } from '../../redux/slices/game-state-slice';
import { JoinGameResponse } from '../../hub/HubInterfaces';

function LobbyView() {
  const hub = useContext(ConnectionHubContext);
  const httpRequestHandler = new HttpRequestHandler();
  const player = useAppSelector((state) => state.player);
  const playerScores = useAppSelector((state) => state.gameState.playerScore);
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const isInitialEffectRender = useRef(true);

  const [isLobbyDisplayed, setLobbyDisplayed] = useState(false);
  const [isPlayerHost, setIsPlayerHost] = useState(false);
  const [invitationUrl, setInvitationUrl] = useState(player.gameHash);
  const [activeButton, setActiveButton] = useState(true);

  const handleStartGameButtonClick = async () => {
    await hub.invoke(HubEvents.startGame, player.token, player.gameHash);
  }

  const handleLeaveGameButtonClick = async () => {
    await hub.invoke(HubEvents.leaveGame, player.token, player.gameHash);
    navigate(config.mainClientEndpoint);
  }

  useEffect(() => {
    if (!isInitialEffectRender.current) {
      return;
    }

    const setHub = async () => {
      const getJoinGameResponse = (dataSerialized: string) => {

        const response = JSON.parse(dataSerialized) as JoinGameResponse;
        dispatch(updatedPlayerList(response.playerScores));
      }

      hub.on(HubEvents.onPlayerJoinedGame, getJoinGameResponse);
      hub.on(HubEvents.onPlayerLeftGame, getJoinGameResponse);

      await hub.start();
      await hub.invoke(HubEvents.joinGame, player.token, player.gameHash, player.username);
    }

    const runChecks = async() => {
  
      const checkIfPlayerIsHost = async () => {
        const isPlayerHost = await httpRequestHandler.fetchPlayerIsHost(player.token, player.gameHash);

        if (isPlayerHost === true) {
          setIsPlayerHost(isPlayerHost);
        }
        else {
          setIsPlayerHost(false);
        }
      }
  
      await checkIfPlayerIsHost();
    }

    setHub();
    //runChecks();
    setLobbyDisplayed(true);

    const clearBeforeUnload = () => {
      hub.off("PlayerJoinedLobby");
      hub.off("PlayerLeftLobby");
      hub.send(HubEvents.leaveGame, player.token, player.gameHash);
      hub.stop();
    }

    window.addEventListener("beforeunload", clearBeforeUnload);
    isInitialEffectRender.current = false;

    return () => {
      clearBeforeUnload();
      window.removeEventListener("beforeunload", clearBeforeUnload);
    }
  }, []);

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
    <>
      { isLobbyDisplayed && 
        <div className="container mb-3">
          <div className="row col-lg-6 col-sm-12 mx-auto text-center">
          </div>
          <div className="row">
            <div className="col-lg-4 col-sm-5 col-12 mx-auto mt-2 text-center order-lg-1 order-2 mb-3">
              <div className="col-lg-6">
                <PlayerList
                  title={"Players in the lobby"}
                  displayPoints={false}
                  displayIndex={false}
                />
              </div>
            </div>
            <div className="col-lg-4 col-sm-10 col-12 mx-auto text-center order-lg-2 order-1">
              <Alert />
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
      }
    </>
  );
}

export default LobbyView;