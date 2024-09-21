import { useContext, useEffect } from "react";
import { BsPaletteFill, BsShieldShaded } from "react-icons/bs";
import { useDispatch } from "react-redux";
import { updatedCorrectGuessPlayerUsernames, updatedCurrentRound, updatedDrawingPlayerUsername } from "../../redux/slices/game-state-slice";
import HubEvents from "../../hub/HubMessages";
import { ConnectionHubContext } from "../../context/ConnectionHubContext";
import { useAppSelector } from "../../redux/hooks";
import * as signalR from '@microsoft/signalr';
import { animated, useSpring } from "@react-spring/web";
import { PlayerScore } from "../../interfaces/PlayerScore";

interface PlayerScoresProps {
  title: string,
  playerScores: PlayerScore[],
  displayPoints: boolean,
  displayIndex: boolean,
  displayRound: boolean
}

function PlayerScores({title, playerScores, displayPoints, displayIndex, displayRound}: PlayerScoresProps) {
  const hub = useContext(ConnectionHubContext);
  const dispatch = useDispatch();
  const player = useAppSelector((state) => state.player);
  const gameState = useAppSelector((state) => state.gameState);
  const gameSettings = useAppSelector((state) => state.gameSettings);
  
  useEffect(() => {
    if (hub.getState() !== signalR.HubConnectionState.Connected) {
      return;
    }

    hub.on(HubEvents.onUpdateCurrentRound, (currentRound: number) => {
      dispatch(updatedCurrentRound(currentRound));
    });

    hub.on(HubEvents.onUpdateDrawingPlayer, (drawingPlayerUsername: string) => {
      dispatch((updatedDrawingPlayerUsername(drawingPlayerUsername)));
    });

    hub.on(HubEvents.onUpdateCorrectGuessPlayerUsernames, (usernamesSerialized: string) => {
      const usernames = JSON.parse(usernamesSerialized) as string[];
  
      dispatch((updatedCorrectGuessPlayerUsernames(usernames)));
    });

    return () => {
      hub.off(HubEvents.onUpdateCurrentRound);
      hub.off(HubEvents.onUpdateDrawingPlayer);
      hub.off(HubEvents.onUpdateCorrectGuessPlayerUsernames);
    }
  }, [hub.getState()]);

  const playerScoresAnimationSpring = useSpring({
    from: { x: -200 },
    to: { x: 0 },
  });

  return (
    <animated.div style={{ ...playerScoresAnimationSpring }}>
      {displayRound && <h5>Round {gameState.currentRound}/{gameSettings.roundsCount}</h5>}
      <ul className="list-group">
        <li className="list-group-item justify-content-between align-items-center">
          <b>{title}</b>
        </li>
        {playerScores.map((playerScore, index) => (
          <li
            key={index}
            className={`list-group-item d-flex justify-content-between align-items-center ${
              gameState.correctGuessPlayerUsernames !== undefined &&
              gameState.correctGuessPlayerUsernames.includes(playerScore.username)
                ? "bg-custom-lime"
                : gameState.drawingPlayerUsername === playerScore.username
                ? "bg-light"
                : "bg-white"
            }`}
            style={{ overflowWrap: "break-word" }}
          >
            {displayIndex && "#" + (index + 1)}
            <div className="d-flex flex-column align-items-center">
              <span className={player.username === playerScore.username ? "text-warning" : "text-dark"}>
                <b>{playerScore.username} </b>
                {gameState.hostPlayerUsername === playerScore.username && <BsShieldShaded />}
                {gameState.drawingPlayerUsername === playerScore.username && <BsPaletteFill />}
              </span>
              {displayPoints && (
                <span className="badge rounded-pill bg-dark mt-2">{playerScore.score} points</span>
              )}
            </div>
            <div></div>
          </li>
        ))}
      </ul>
    </animated.div>
  );
}

export default PlayerScores;