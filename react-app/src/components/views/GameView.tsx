import Chat from '../Chat';
import PlayerList from '../PlayerList';
import Canvas from '../Canvas';
import { Player } from '../../redux/slices/player-slice';
import ControlPanel from '../ControlPanel';
import { useAppSelector } from '../../redux/hooks';
import { useContext, useEffect, useState } from 'react';
import { ConnectionHubContext } from '../../context/ConnectionHubContext';
import HubEvents from '../../hub/HubEvents';
import useLocalStorage from 'use-local-storage';

function GameView() {
  const gameHub = useContext(ConnectionHubContext);
  const gameSettings = useAppSelector((state) => state.gameSettings);
  const gameState = useAppSelector((state) => state.gameState);;
  const player = useAppSelector((state) => state.player);
  const [localStorageGameHash, setLocalStorageGameHash] = useLocalStorage("gameHash", "")
  const [playerList, setPlayerList] = useState<Player[]>([]);
  
  useEffect(() => {
      const setConnectionHub = async () => {
  
        const getPlayerList = (playerListSerialized: any) => {
          const playerList = JSON.parse(playerListSerialized) as Player[];
  
          setPlayerList(playerList);
        }
        
        gameHub.on(HubEvents.onPlayerJoinedGame, getPlayerList);
        gameHub.on(HubEvents.onPlayerLeftGame, getPlayerList);
  
        await gameHub.start();
        await gameHub.invoke(HubEvents.joinGame, localStorageGameHash, player.username);
      }
  
      const clearBeforeUnload = () => {
        gameHub.off(HubEvents.onPlayerJoinedGame);
        gameHub.off(HubEvents.onPlayerLeftGame);
        gameHub.send(HubEvents.leaveGame, localStorageGameHash, player.username);
      }
  
      setConnectionHub();
  
      window.addEventListener("beforeunload", clearBeforeUnload);
  
      return () => {
        clearBeforeUnload();
        window.removeEventListener("beforeunload", clearBeforeUnload);
      }
    }, []);

  return (
    <div className="container text-center">
      <div className="row">
        <div className="col-lg-2 col-md-6 col-12 order-lg-1 order-md-2 order-3 mb-3">
          <PlayerList
            title={"Players"}
            players={playerList}
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
  )
}

export default GameView;