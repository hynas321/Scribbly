import { useAppSelector } from "../../../redux/hooks";
import { animated, useSpring } from "@react-spring/web";
import { PlayerScore } from "../../../interfaces/PlayerScore";
import { useTranslation } from "../../../i18n/I18nContext";
import PlayerScoreList from "./PlayerScoreList";

interface PlayerScoresProps {
  title: string;
  playerScores: PlayerScore[];
  displayPoints: boolean;
  displayIndex: boolean;
  displayRound: boolean;
}

const PlayerScores = ({
  title,
  playerScores,
  displayPoints,
  displayIndex,
  displayRound,
}: PlayerScoresProps) => {
  const { t } = useTranslation();
  const gameState = useAppSelector((state) => state.gameState);
  const gameSettings = useAppSelector((state) => state.gameSettings);

  const playerScoresAnimationSpring = useSpring({
    from: { x: -200 },
    to: { x: 0 },
  });

  return (
    <animated.div style={{ ...playerScoresAnimationSpring }}>
      {displayRound && (
        <h5>
          {t("game.round", { current: gameState.currentRound, total: gameSettings.roundsCount })}
        </h5>
      )}
      <PlayerScoreList
        title={title}
        playerScores={playerScores}
        displayPoints={displayPoints}
        displayIndex={displayIndex}
      />
    </animated.div>
  );
};

export default PlayerScores;
