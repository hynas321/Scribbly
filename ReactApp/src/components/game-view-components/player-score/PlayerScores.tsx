import { useContext } from "react";
import { ConnectionHubContext } from "../../../context/ConnectionHubContext";
import { useAppSelector } from "../../../redux/hooks";
import { animated, useSpring } from "@react-spring/web";
import { PlayerScore } from "../../../interfaces/PlayerScore";
import { usePlayerScoresHubEvents } from "../../../hooks/hub-events/usePlayerScoresHubEvents";
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
  const hub = useContext(ConnectionHubContext);
  const gameState = useAppSelector((state) => state.gameState);
  const gameSettings = useAppSelector((state) => state.gameSettings);

  const playerScoresAnimationSpring = useSpring({
    from: { x: -200 },
    to: { x: 0 },
  });

  usePlayerScoresHubEvents(hub);

  return (
    <animated.div style={{ ...playerScoresAnimationSpring }}>
      {displayRound && (
        <h5>
          Round {gameState.currentRound}/{gameSettings.roundsCount}
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
}

export default PlayerScores;
