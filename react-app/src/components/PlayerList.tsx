import { Player } from "../redux/slices/player-slice";

interface PlayerListProps {
  title: string,
  players: Player[],
  displayPoints: boolean,
  displayIndex: boolean,
  round?: Round
}

function PlayerList({title, players, displayPoints, displayIndex, round}: PlayerListProps) {
  return (
    <>
      { round && <h5>Round {round.currentRound}/{round.roundCount}</h5>}
      <ul className="list-group">
        <li className="list-group-item justify-content-between align-items-center">
            <b>{title}</b>
        </li>
        {players.map((player, index) => (
          <li key={index} className="list-group-item d-flex justify-content-between align-items-center">
            { displayIndex && index + 1 + "."} {player.username}
            { displayPoints && <span className="badge rounded-pill bg-dark">{player.score}</span> }
          </li>
          ))}
      </ul>
    </>
  )
}

export default PlayerList;