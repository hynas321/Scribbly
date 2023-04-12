import Chat from '../Chat';
import PlayerList from '../PlayerList';
import Canvas from '../Canvas';
import { useState } from 'react';
import { Player } from '../../redux/slices/player-slice';
import ControlPanel from '../ControlPanel';
import { useAppDispatch, useAppSelector } from '../../redux/hooks';

function GameView() {
  const player: Player = {
    username: "Player",
    points: 100
  }

  const gameSettings = useAppSelector((state) => state.gameSettings);
  const gameState = useAppSelector((state) => state.gameState);
  const dispatch = useAppDispatch();

  const wordLength = "Testing".length; //will be fetched from the word API
  const [players, setPlayers] = useState<Player[]>([player, player, player, player, player, player, player, player]);

  return (
    <div className="container text-center">
      <div className="row">
        <div className="col-2">
          <PlayerList 
            players={players}
            currentRound={gameState.currentRound}
            roundCount={gameSettings.roundsCount}
          />
          <ControlPanel />
        </div>
        <div className="col-7">
          <Canvas
            currentProgress={gameState.currentDrawingTimeSeconds}
            minProgress={0}
            maxProgress={gameSettings.drawingTimeSeconds}
          />
        </div>
        <div className="col-3">
          <Chat
            wordLength={wordLength}
          />
        </div>
      </div>
    </div>
  )
}

export default GameView;