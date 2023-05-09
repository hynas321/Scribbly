interface ScoreboardProps {
  title: string,
  playerScores: PlayerScore[],
  displayPoints: boolean,
  displayIndex: boolean,
}

function Scoreboard({title, playerScores, displayPoints, displayIndex}: ScoreboardProps) {
  return (
    <>
      <ul className="list-group">
        <li className="list-group-item justify-content-between align-items-center">
            <b>{title}</b>
        </li>
        {playerScores.map((playerScore, index) => (
          <li key={index} className="list-group-item d-flex justify-content-between align-items-center">
            { displayIndex && index + 1 + "."}
            <span className="text-dark">{playerScore.username}</span>
            { displayPoints && <span className="badge rounded-pill bg-dark">{playerScore.score}</span> }
          </li>
          ))}
      </ul>
    </>
  )
}

export default Scoreboard;