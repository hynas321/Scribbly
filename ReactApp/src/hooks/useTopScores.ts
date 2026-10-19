import { useEffect, useState } from "react";
import { MainScoreboardScore } from "../interfaces/MainScoreboardScore";
import api from "../http/Api";
import { reportRealtimeError } from "../utils/reportRealtimeError";
import { useTranslation } from "../i18n/I18nContext";

export const useTopScores = () => {
  const { t } = useTranslation();
  const [scoreboardScores, setScoreboardScores] = useState<MainScoreboardScore[]>([]);
  const [isTableDisplayed, setIsTableDisplayed] = useState(false);
  useEffect(() => {
    let active = true;
    void api
      .fetchTopAccountScores()
      .then((scores) => {
        if (active) {
          setScoreboardScores(scores);
        }
      })
      .catch(() => {
        if (active) {
          reportRealtimeError(t("error.action.loadScoreboard"));
        }
      })
      .finally(() => {
        if (active) {
          setIsTableDisplayed(true);
        }
      });
    return () => {
      active = false;
    };
  }, [t]);
  return { scoreboardScores, isTableDisplayed };
};
