import { useEffect, useRef, useState } from 'react';
import Button from '../Button';
import GameSettingsBoard from '../GameSettingsBoard';
import { useAppSelector } from '../../redux/hooks';
import config from '../../../config.json';
import Alert from '../Alert';
import { Link, useNavigate } from 'react-router-dom';
import PlayerList from '../PlayerList';
import Chat from '../Chat';
import { BsPlayCircle, BsDoorOpen } from 'react-icons/bs';
import { Player } from '../../redux/slices/player-slice';
import { useContext } from "react";
import { ConnectionHubContext } from '../../context/ConnectionHubContext';
import HubEvents from '../../hub/HubEvents';
import Canvas from '../Canvas';
import ControlPanel from '../ControlPanel';
import { useDispatch } from 'react-redux';
import { updatedPlayerScores } from '../../redux/slices/game-state-slice';
import VerificationHelper from '../../utils/VerificationHelper';
import HttpRequestHandler from '../../http/HttpRequestHandler';
import { updatedAlert, updatedVisible } from '../../redux/slices/alert-slice';

function GameView() {
  const hub = useContext(ConnectionHubContext);
  const verificationHelper = new VerificationHelper();
  const httpRequestHandler = new HttpRequestHandler();
  const player = useAppSelector((state) => state.player);
  const alert = useAppSelector((state) => state.alert);
  const gameState = useAppSelector((state) => state.gameState);
  const gameSettings = useAppSelector((state) => state.gameSettings);
  const navigate = useNavigate();
  const dispatch = useDispatch();
  
  const isInitialEffectRender = useRef(true);
  

  const [isGameStarted, setGameStarted] = useState(false);
  const [activeButton, setActiveButton] = useState(true);
  const isPlayerHost = true;

  const handleStartGameButtonClick = async () => {

    navigate(config.gameClientEndpoint);
  }

  useEffect(() => {
    if (!isInitialEffectRender.current) {
      return;
    }

    const setLobbyHub = async () => {
      const getPlayerList = (playerListSerialized: any) => {
        const playerScore = JSON.parse(playerListSerialized) as Player[];
        dispatch(updatedPlayerScores(playerScore));
      }
      
      hub.on(HubEvents.onPlayerJoinedGame, getPlayerList);
      hub.on(HubEvents.onPlayerLeftGame, getPlayerList);

      await hub.start();
      await hub.invoke(HubEvents.joinGame, player.token, player.username);
    }

    const checkIfGameExists = async () => {
      await httpRequestHandler.checkIfGameExists()
        .then((data: boolean) => {
  
          if (typeof data != "boolean") {
            displayAlert("Unexpected error, try again", "danger");
            navigate(config.mainClientEndpoint);
            return;
          }
  
          if (data === false) {
            displayAlert("Game does not exist", "danger");
            navigate(config.mainClientEndpoint);
          }
        })
        .catch(() => {
          displayAlert("Unexpected error, try again", "danger");
          navigate(config.mainClientEndpoint);
        });
    }

    const checkIfPlayerExists = async () => {
      
      await httpRequestHandler.checkIfPlayerExists()
        .then((data: boolean)) => {

          if (typeof data != "boolean") {
            displayAlert("Unexpected error, try again", "danger");
            navigate(config.mainClientEndpoint);
            return;
          }
        }
    }

    const clearBeforeUnload = () => {
      hub.off(HubEvents.onPlayerJoinedGame);
      hub.off(HubEvents.onPlayerLeftGame);
      hub.send(HubEvents.leaveGame, player.token);
      hub.stop();
    }

    setLobbyHub();
    checkIfGameExists();

    window.addEventListener("beforeunload", clearBeforeUnload);

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
      {
        isGameStarted ?

          <div className="container text-center">
            <div className="row">
              <div className="col-lg-2 col-md-6 col-12 order-lg-1 order-md-2 order-3 mb-3">
                <PlayerList
                  title={"Players"}
                  displayPoints={true}
                  displayIndex={true}
                  round={{
                    currentRound: gameState.currentRound,
                    roundCount: gameSettings.roundsCount
                  }}
                />
                <ControlPanel />
              </div>
              <div className="col-lg-7 col-md-12 col-12 order-lg-1 order-md-1 order-1 mb-3">
                <Canvas
                  progressBarProperties={{
                    currentProgress: gameState.currentDrawingTimeSeconds,
                    minProgress: 0,
                    maxProgress: gameSettings.drawingTimeSeconds
                  }}
                />
              </div>
              <div className="col-lg-3 col-md-6 col-12 order-lg-1 order-md-3 order-2 mb-3">
                <Chat
                  placeholderValue="Enter your guess"
                  wordLength={gameState.wordLength}
                />
              </div>
            </div>
          </div> 

          :

          <div className="container mb-3">
          <div className="row col-lg-6 col-sm-12 mx-auto text-center">
            <Alert/>
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
                <Chat placeholderValue={"Enter your message"}/>
              </div>
            </div>
          </div>
        </div>
      }
    </>
  );
}

export default GameView;
