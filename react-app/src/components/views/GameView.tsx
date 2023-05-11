import { useEffect, useState } from 'react';
import Button from '../Button';
import GameSettingsBoard from '../game-view-components/GameSettingsBoard';
import { useAppSelector } from '../../redux/hooks';
import config from '../../../config.json';
import { useNavigate } from 'react-router-dom';
import PlayerList from '../game-view-components/PlayerList';
import Chat from '../game-view-components/Chat';
import { BsPlayCircle, BsDoorOpen } from 'react-icons/bs';
import { PlayerScore, updatedPlayerScore } from '../../redux/slices/player-score-slice';
import { useContext } from "react";
import { ConnectionHubContext, LongRunningConnectionHubContext } from '../../context/ConnectionHubContext';
import HubEvents from '../../hub/HubEvents';
import Canvas from '../game-view-components/Canvas';
import ControlPanel from '../ControlPanel';
import { useDispatch } from 'react-redux';
import HttpRequestHandler from '../../http/HttpRequestHandler';
import { updatedAlert, updatedVisible } from '../../redux/slices/alert-slice';
import { PlayerIsHostResponse } from '../../http/HttpInterfaces';
import useLocalStorageState from 'use-local-storage-state';
import { GameSettings, updatedGameSettings } from '../../redux/slices/game-settings-slice';
import loading from './../../assets/loading.gif'
import { GameState, clearedGameState, updatedGameState } from '../../redux/slices/game-state-slice';

function GameView() {
  const httpRequestHandler = new HttpRequestHandler();

  const hub = useContext(ConnectionHubContext);
  const longRunningHub = useContext(LongRunningConnectionHubContext);

  const navigate = useNavigate();
  const dispatch = useDispatch();

  const player = useAppSelector((state) => state.player);
  const gameSettings = useAppSelector((state) => state.gameSettings);

  const [playerScores, setPlayerScores] = useState<PlayerScore[]>([]);
  const [isGameStarted, setIsGameStarted] = useState<boolean>(false);
  const [isPlayerHost, setIsPlayerHost] = useState<boolean>(false);
  const [isGameDisplayed, setIsGameDisplayed] = useState<boolean>(false);
  const [isStartGameButtonActive, setIsStartGameButtonActive] = useState<boolean>(false);

  const [token, setToken] = useLocalStorageState("token", { defaultValue: "" });
  const [username, setUsername] = useLocalStorageState("username", { defaultValue: ""});

  const handleStartGameButtonClick = async () => {
    await longRunningHub.start();
    await longRunningHub.invoke(HubEvents.startGame, token, gameSettings);
    await longRunningHub.stop();
  }

  const handleLeaveGameButtonClick = async () => {
    await hub.invoke(HubEvents.leaveGame, token, true);
    setToken("");
    navigate(config.mainClientEndpoint);
  }

  useEffect(() => {
    const startHubAndJoinGame = async () => {
      hub.on(HubEvents.onUpdatePlayerScores, (playerScoresSerialized: string) => {
        const playerScores = JSON.parse(playerScoresSerialized) as PlayerScore[];
        setPlayerScores(playerScores);
      });

      hub.on(HubEvents.onStartGame, () => {
        setIsGameStarted(true)
      });

      hub.on(HubEvents.onJoinGame, (
        playerSerialized: string,
        gameSettingsSerialized: string,
        gameStateSerialized: string
      ) => {
        const player = JSON.parse(playerSerialized) as Player;
        const settings = JSON.parse(gameSettingsSerialized) as GameSettings;
        const state = JSON.parse(gameStateSerialized) as GameState;

        dispatch(updatedPlayerScore({username: player.username, score: player.score}));
        dispatch(updatedGameSettings(settings));
        dispatch(updatedGameState(state));
        setToken(player.token);

        setTimeout(() => {
          setIsGameDisplayed(true);
        }, 1000);
      });

      hub.on(HubEvents.onJoinGameError, (errorMessage: string) => {
        displayAlert(errorMessage, "danger");
        navigate(config.mainClientEndpoint);
      });

      hub.on(HubEvents.onGameProblem, (announcementMessage: string) => {
        const problemMessage = JSON.parse(announcementMessage) as AnnouncementMessage;

        displayAlert(problemMessage.text, problemMessage.bootstrapBackgroundColor);
        navigate(config.mainClientEndpoint);
      });

      hub.on(HubEvents.onEndGame, () => {
        displayAlert("Game has ended", "success");
        navigate(config.mainClientEndpoint);
      });

      const regex: RegExp = /^Player \d{4}$/;
      const playerUsername = regex ? username : player.username;

      await hub.start();
      await hub.invoke(HubEvents.joinGame, token, playerUsername);
    }

    const clearBeforeUnload = async () => {
      hub.off(HubEvents.onUpdatePlayerScores);
      hub.off(HubEvents.onStartGame);
      hub.off(HubEvents.onJoinGame);
      hub.off(HubEvents.onJoinGameError);
      hub.off(HubEvents.onGameProblem);
      dispatch(clearedGameState());
      await hub.invoke(HubEvents.leaveGame, token, false);
    }

    startHubAndJoinGame();

    window.addEventListener("beforeunload", clearBeforeUnload);
    window.addEventListener("unload", clearBeforeUnload);

    return () => {
      clearBeforeUnload();
      window.removeEventListener("beforeunload", clearBeforeUnload);
      window.removeEventListener("unload", clearBeforeUnload);
    }
  }, []);

  useEffect(() => {
      const checkIfPlayerIsHost = async () => {
        await httpRequestHandler.checkIfPlayerIsHost(token)
        .then((data: PlayerIsHostResponse) => {

          if (data.isHost === true) {
            setIsPlayerHost(true);
          }
        })
      }
      
      checkIfPlayerIsHost();
  }, [token]);

  useEffect(() => {
    if (playerScores.length > 1) {
      setIsStartGameButtonActive(true);
    }
    else {
      setIsStartGameButtonActive(false);
    }
  }, [playerScores]);

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
        !isGameDisplayed ? 
          <div className='d-flex justify-content-center align-items-center mt-4'>
            <img src={loading} alt="Loading" className="w-30 h-30 img-fluid" />
          </div>
        : 
          (
            isGameStarted ?

            <div className="container text-center">
              <div className="row">
                <div className="col-lg-2 col-md-6 col-12 order-lg-1 order-md-2 order-3 mb-3">
                  <PlayerList
                    title={"Players"}
                    playerScores={playerScores}
                    displayPoints={true}
                    displayIndex={true}
                    displayRound={true}
                  />
                  <ControlPanel onClick={handleLeaveGameButtonClick} />
                </div>
                <div className="col-lg-7 col-md-12 col-12 order-lg-1 order-md-1 order-1 mb-3">
                  <Canvas />
                </div>
                <div className="col-lg-3 col-md-6 col-12 order-lg-1 order-md-3 order-2 mb-3">
                  <Chat 
                    placeholderValue="Enter your guess"
                    displaySecretWord={true}
                  />
                </div>
              </div>
            </div> 

            :

            <div className="container mb-3">
            <div className="row">
              <div className="col-lg-4 col-sm-5 col-12 mx-auto mt-2 text-center order-lg-1 order-2 mb-3">
                <div className="col-lg-6">
                  <PlayerList
                    title={"Players in the lobby"}
                    playerScores={playerScores}
                    displayPoints={false}
                    displayIndex={false}
                    displayRound={false}
                  />
                </div>
              </div>
              <div className="col-lg-4 col-sm-10 col-12 mx-auto text-center order-lg-2 order-1">
                <h5>Your username: {player.username}</h5>
                { isPlayerHost && 
                  <Button
                    text="Start the game"
                    type="success"
                    active={isStartGameButtonActive}
                    icon={<BsPlayCircle/>}
                    onClick={handleStartGameButtonClick}
                  />
                }
                { !isPlayerHost && <h4 className="mt-3">Waiting for the host to start the game</h4> }
                  <Button
                    text="Leave the game"
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
                  <Chat
                    placeholderValue={"Enter your message"}
                    displaySecretWord={false}
                  />
                </div>
              </div>
            </div>
          </div>
        )
      }
    </>
  );
}

export default GameView;
