import { MainScoreboardScore } from "../interfaces/MainScoreboardScore";
import { useTranslation } from "../i18n/I18nContext";

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
  const { t } = useTranslation();
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
              key={`${scoreboardScore.name}-${index}`}
              className="list-group-item d-flex justify-content-between align-items-center"
            >
              {displayIndex && "#" + (index + 1)}
              <div className="d-flex flex-column align-items-center">
                <span>{scoreboardScore.name}</span>
                {displayPoints && (
                  <span className="badge rounded-pill bg-dark mt-2">
                    {t("common.scorePoints", { score: scoreboardScore.score })}
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
