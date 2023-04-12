import { Player } from "../redux/slices/player-slice";

interface PlayerListProps {
  players: Player[],
  currentRound: number,
  roundCount: number
}

function PlayerList({players, currentRound, roundCount}: PlayerListProps) {
    return (
      <>
        <h5>Round {currentRound}/{roundCount}</h5>
        <ul className="list-group">
          <li className="list-group-item justify-content-between align-items-center">
              Players
          </li>
          {players.map((player, index) => (
            <li key={index} className="list-group-item d-flex justify-content-between align-items-center">
              {player.username}
              <span className="badge rounded-pill bg-dark">{player.points}</span>
            </li>
            ))}
        </ul>
      </>
    )
}

export default PlayerList;