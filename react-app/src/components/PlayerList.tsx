import { useEffect } from "react";
import { useAppSelector } from "../redux/hooks";

interface PlayerListProps {
  title: string,
  playerScores: PlayerScore[],
  displayPoints: boolean,
  displayIndex: boolean,
  username?: string,
  round?: Round
}

function PlayerList({title, playerScores, displayPoints, displayIndex, username, round}: PlayerListProps) {
  useEffect(() => {
    console.log(playerScores);
  }, []);

  return (
    <>
      { round && <h5>Round {round.currentRound}/{round.roundCount}</h5>}
      <ul className="list-group">
        <li className="list-group-item justify-content-between align-items-center">
            <b>{title}</b>
        </li>
        {playerScores.map((playerScore, index) => (
          <li key={index} className="list-group-item d-flex justify-content-between align-items-center">
            { displayIndex && index + 1 + "."}
            <span className={username == playerScore.username ? "text-warning" : "text-dark"}>{playerScore.username}</span>
            { displayPoints && <span className="badge rounded-pill bg-dark">{playerScore.score}</span> }
          </li>
          ))}
      </ul>
    </>
  )
}

export default PlayerList;