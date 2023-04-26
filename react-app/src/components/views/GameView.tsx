import Chat from '../Chat';
import PlayerList from '../PlayerList';
import Canvas from '../Canvas';
import { useContext, useEffect, useState } from 'react';
import { Player } from '../../redux/slices/player-slice';
import ControlPanel from '../ControlPanel';
import { useAppSelector } from '../../redux/hooks';
import { HubType } from '../../enums/HubType';
import { GameHubContext } from '../../context/GameHubContext';
import HubEvents from '../../hub/HubEvents';

function GameView() {
  const gameHub = useContext(GameHubContext);
  const gameSettings = useAppSelector((state) => state.gameSettings);
  const gameState = useAppSelector((state) => state.gameState);
  const player = useAppSelector((state) => state.player);

  const [playerList, setPlayerList] = useState<Player[]>([]);
  const wordRiddleLength = 10; //will be fetched from the server
  const testGameHash = "TestGameHash"; //temporary

  useEffect(() => {
    const setGameHub = async () => {

      const getPlayerList = (playerListSerialized: any) => {
        const playerList = JSON.parse(playerListSerialized) as Player[];

        setPlayerList(playerList);
      }
      
      gameHub.on(HubEvents.onPlayerJoinedGame, getPlayerList);
      gameHub.on(HubEvents.onPlayerLeftGame, getPlayerList);

      await gameHub.start();
      await gameHub.invoke(HubEvents.joinGame, testGameHash, player.username);
    }

    const clearBeforeUnload = () => {
      gameHub.off(HubEvents.onPlayerJoinedGame);
      gameHub.off(HubEvents.onPlayerLeftGame);
      gameHub.send(HubEvents.leaveGame, testGameHash, player.username);
      gameHub.stop();
    }

    setGameHub();

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
            hubType={HubType.GAME}
            placeholderValue="Enter your guess"
            wordLength={wordRiddleLength}
          />
        </div>
      </div>
    </div>
  )
}

export default GameView;