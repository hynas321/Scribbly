import Chat from '../Chat';
import PlayerList from '../PlayerList';
import Canvas from '../Canvas';
import { useContext, useEffect, useState } from 'react';
import { Player } from '../../redux/slices/player-slice';
import ControlPanel from '../ControlPanel';
import { useAppSelector } from '../../redux/hooks';
import { HubType } from '../../enums/HubType';
import { GameHubContext } from '../../Context/GameHubContext';

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
      
      gameHub.on("PlayerJoinedGame", getPlayerList);
      gameHub.on("PlayerLeftGame", getPlayerList);

      await gameHub.start();
      await gameHub.invoke("JoinGame", testGameHash, player.username);
    }

      const clearBeforeUnload = () => {
        gameHub.off("PlayerJoinedGame");
        gameHub.off("PlayerLeftGame");
        gameHub.send("LeaveGame", testGameHash, player.username);
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
        <div className="col-2">
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
        <div className="col-7">
          <Canvas
            progressBarProperties={{
              currentProgress: gameState.currentDrawingTimeSeconds,
              minProgress: gameSettings.finishRoundSeconds,
              maxProgress: gameSettings.drawingTimeSeconds
            }}
          />
        </div>
        <div className="col-3">
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