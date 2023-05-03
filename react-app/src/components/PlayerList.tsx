import { useEffect, useRef, useState } from "react";
import { useAppSelector } from "../redux/hooks";

interface PlayerListProps {
  title: string,
  displayPoints: boolean,
  displayIndex: boolean,
  round?: Round
}

function PlayerList({title, displayPoints, displayIndex, round}: PlayerListProps) {
  const playerScore = useAppSelector((state) => state.gameState.playerScore);
  const [playerScores, setPlayerScores] = useState<PlayerScore[]>(playerScore);
  const isInitialEffectRender = useRef(true);
  
  useEffect(() => {
    if (!isInitialEffectRender.current) {
      return;
    }

    if (playerScores.length < 1) {
      setPlayerScores([]);
    }

    isInitialEffectRender.current = false;
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
            { displayIndex && index + 1 + "."} {playerScore.username}
            { displayPoints && <span className="badge rounded-pill bg-dark">{playerScore.score}</span> }
          </li>
          ))}
      </ul>
    </>
  )
}

export default PlayerList;