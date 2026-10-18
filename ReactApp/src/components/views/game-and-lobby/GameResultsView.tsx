import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { BsArrowClockwise, BsHouseDoor, BsTrophyFill } from "react-icons/bs";
import config from "../../../../config.json";
import api from "../../../http/Api";
import { CompletedGameResult } from "../../../http/HttpInterfaces";
import { useSessionStorage } from "../../../hooks/useSessionStorage";
import { TranslationKey, useTranslation } from "../../../i18n/I18nContext";
import { rankScores } from "../../../features/game-session/rankScores";
import "./GameResultsView.css";

const placementKeys: Record<number, TranslationKey> = {
  1: "results.first",
  2: "results.second",
  3: "results.third",
};

export default function GameResultsView() {
  const { gameHash = "" } = useParams();
  const { t } = useTranslation();
  const { resultGameHash, resultRoomAccessToken, activeGameHash, roomAccessToken } =
    useSessionStorage();
  const token =
    resultGameHash === gameHash
      ? resultRoomAccessToken
      : activeGameHash === gameHash
        ? roomAccessToken
        : "";
  const [result, setResult] = useState<CompletedGameResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [failed, setFailed] = useState(false);
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    let active = true;
    setResult(null);
    setFailed(false);
    setLoading(true);
    if (!token) {
      setLoading(false);
      return;
    }
    void api
      .getCompletedGameResult(gameHash, token)
      .then((value) => {
        if (active) {
          setResult(value);
        }
      })
      .catch(() => {
        if (active) {
          setFailed(true);
        }
      })
      .finally(() => {
        if (active) {
          setLoading(false);
        }
      });
    return () => {
      active = false;
    };
  }, [gameHash, token, attempt]);

  const standings = rankScores(result?.scores ?? []);
  const winners = standings.filter((player) => player.place === 1);
  const podium = standings.filter((player) => player.place <= 3);
  const displayName = (name: string | null, index: number) => {
    return name || t("results.playerFallback", { number: index + 1 });
  };
  const ready = result?.completed && standings.length > 0;

  return (
    <main className="container results-page pb-5">
      <header
        className="d-flex align-items-center justify-content-between flex-wrap gap-3
          border-bottom py-3 mb-4"
      >
        <h2 className="h4 mb-0">{t("results.title")}</h2>
        <Link className="btn btn-success" to={config.mainClientEndpoint}>
          <BsHouseDoor aria-hidden="true" /> {t("results.home")}
        </Link>
      </header>
      {loading ? (
        <p className="text-center text-muted py-5" role="status">
          {t("app.loading")}
        </p>
      ) : !ready ? (
        <div
          className={`alert ${failed ? "alert-danger" : "alert-secondary"} text-center`}
          role={failed ? "alert" : "status"}
        >
          <p>{t(failed ? "results.loadError" : "results.unavailable")}</p>
          {token && (
            <button
              className="btn btn-success"
              type="button"
              onClick={() => setAttempt((value) => value + 1)}
            >
              <BsArrowClockwise aria-hidden="true" /> {t("results.retry")}
            </button>
          )}
        </div>
      ) : (
        <>
          <section
            className="results-winner bg-light text-center py-4 px-3 mb-3"
            aria-labelledby="winner-title"
          >
            <BsTrophyFill className="text-warning fs-2 mb-2" aria-hidden="true" />
            <p className="text-muted mb-1" id="winner-title">
              {t(winners.length > 1 ? "results.jointWinners" : "results.winner")}
            </p>
            <h3 className="h4 text-success mb-2">
              {winners.map((player, index) => displayName(player.username, index)).join(" & ")}
            </h3>
            <p className="mb-0">
              <span className="badge bg-dark">
                {t("common.scorePoints", { score: winners[0].score })}
              </span>
            </p>
          </section>
          <section
            className="row row-cols-1 row-cols-md-3 g-3 mb-4"
            aria-label={t("results.podium")}
          >
            {podium.map((player, index) => (
              <div className="col" key={index}>
                <div
                  className="results-placement list-group-item border rounded d-flex
                    align-items-center gap-3 p-3 h-100"
                >
                  <span
                    className={`badge ${
                      player.place === 1 ? "bg-warning text-dark" : "bg-secondary"
                    }`}
                    aria-hidden="true"
                  >
                    {player.place}
                  </span>
                  <div className="results-placement-details">
                    <p className="small text-muted mb-1">{t(placementKeys[player.place])}</p>
                    <h4 className="h6 mb-2">{displayName(player.username, index)}</h4>
                    <span className="badge bg-dark">
                      {t("common.scorePoints", { score: player.score })}
                    </span>
                  </div>
                </div>
              </div>
            ))}
          </section>
          <section aria-labelledby="standings-title">
            <div className="d-flex align-items-center justify-content-between flex-wrap gap-2 mb-3">
              <h3 className="h5 mb-0" id="standings-title">
                {t("results.standings")}
              </h3>
              <span className="badge bg-secondary">
                {t("results.playerCount", { count: standings.length })}
              </span>
            </div>
            <table className="table table-bordered table-hover align-middle results-table mb-0">
              <caption className="visually-hidden">{t("results.standings")}</caption>
              <thead className="table-light">
                <tr>
                  <th scope="col">{t("results.place")}</th>
                  <th scope="col">{t("results.player")}</th>
                  <th scope="col" className="text-end">
                    {t("results.points")}
                  </th>
                </tr>
              </thead>
              <tbody>
                {standings.map((player, index) => (
                  <tr
                    key={index}
                    className={player.isCurrentPlayer ? "results-current-player table-success" : ""}
                  >
                    <td>{player.place}</td>
                    <th scope="row" className="fw-normal">
                      {displayName(player.username, index)}
                      {player.isCurrentPlayer && (
                        <span className="badge bg-success ms-2">{t("results.you")}</span>
                      )}
                    </th>
                    <td className="text-end">{player.score}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </section>
        </>
      )}
    </main>
  );
}
