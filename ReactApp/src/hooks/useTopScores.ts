import { useEffect, useState } from "react";
import { MainScoreboardScore } from "../interfaces/MainScoreboardScore";
import UrlHelper from "../utils/UrlHelper";
import api from "../http/Api";

export const useTopScores = () => {
  const [scoreboardScores, setScoreboardScores] = useState<MainScoreboardScore[]>([]);
  const [, setGameHash] = useState<string>("");
  const [isTableDisplayed, setIsTableDisplayed] = useState(false);

  useEffect(() => {
    const fetchScores = async () => {
      setGameHash(UrlHelper.getGameHash(window.location.href));
      try {
        const data = await api.fetchTopAccountScores();
        if (!Array.isArray(data)) {
          setIsTableDisplayed(false);
          return;
        }
        setScoreboardScores(data);
        setTimeout(() => setIsTableDisplayed(true), 1000);
      } catch (err) {
        console.error(err);
      }
    };
    fetchScores();
  }, []);

  return { scoreboardScores, isTableDisplayed };
};
