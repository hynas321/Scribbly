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
    score: 100
  }

  const gameSettings = useAppSelector((state) => state.gameSettings);
  const gameState = useAppSelector((state) => state.gameState);
  const dispatch = useAppDispatch();

  const [players, setPlayers] =
    useState<Player[]>([player, player, player, player, player, player, player, player]); // will be fetched from the server
  const wordRiddleLength = 10; //will be fetched from the server

  return (
    <div className="container text-center">
      <div className="row">
        <div className="col-2">
          <PlayerList
            title={"Players"}
            players={players}
            displayPoints={true}
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
            placeholderValue="Enter your guess"
            wordLength={wordRiddleLength}
          />
        </div>
      </div>
    </div>
  )
}

export default GameView;