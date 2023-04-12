import { Link } from 'react-router-dom';
import Button from '../Button';
import config from '../../../config.json';
import Chat from '../Chat';
import PlayerList from '../PlayerList';
import Canvas from '../Canvas';
import MatchInfo from '../MatchInfo';
import { useState } from 'react';
import { Player } from '../../redux/slices/player-slice';
import ControlPanel from '../ControlPanel';

function MatchView() {
  const player: Player = {
    username: "Player",
    points: 100
  }

  const [players, setPlayers] = useState<Player[]>([player, player, player, player, player, player, player, player]);

  return (
    <div className="container text-center">
      <div className="row">
        <div className="col-6 mx-auto">
          <MatchInfo 
            currentRound={1}
            roundCount={6}
            currentProgress={75}
            minProgress={0}
            maxProgress={125}
          />
        </div>
      </div>
      <div className="row">
        <div className="col-2">
          <PlayerList 
            players={players} 
          />
          <ControlPanel />
        </div>
        <div className="col-7">
          <Canvas />
        </div>
        <div className="col-3">
          <Chat />
        </div>
      </div>
    </div>
  )
}

export default MatchView;