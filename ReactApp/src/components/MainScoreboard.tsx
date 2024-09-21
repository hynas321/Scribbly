import { MainScoreboardScore } from "../interfaces/MainScoreboardScore";

interface MainScoreboardProps {
  title: string;
  scoreboardScores: MainScoreboardScore[];
  displayPoints: boolean;
  displayIndex: boolean;
}

function MainScoreboard({
  title,
  scoreboardScores,
  displayPoints,
  displayIndex,
}: MainScoreboardProps) {
  return (
    <>
      <ul className="list-group">
        <li className="list-group-item justify-content-between align-items-center">
          <b>{title}</b>
        </li>
        {scoreboardScores.length === 0 ? (
          <li className="list-group-item">-</li>
        ) : (
          scoreboardScores.map((scoreboardScore, index) => (
            <li
              key={index}
              className="list-group-item d-flex justify-content-between align-items-center"
            >
              {displayIndex && "#" + (index + 1)}
              <div className="d-flex flex-column align-items-center">
                <span>
                  {`${scoreboardScore.givenName} (${scoreboardScore.email.split("@")[0]})`}
                </span>
                {displayPoints && (
                  <span className="badge rounded-pill bg-dark mt-2">
                    {scoreboardScore.score} points
                  </span>
                )}
              </div>
              <div></div>
            </li>
          ))
        )}
      </ul>
    </>
  );
}

export default MainScoreboard;
