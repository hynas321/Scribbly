import { useContext, useEffect } from "react";
import { BsPaletteFill, BsShieldShaded } from "react-icons/bs";
import { useDispatch } from "react-redux";
import { updatedCurrentRound, updatedDrawingPlayerUsername } from "../../redux/slices/game-state-slice";
import HubEvents from "../../hub/HubEvents";
import { ConnectionHubContext } from "../../context/ConnectionHubContext";
import { useAppSelector } from "../../redux/hooks";
import * as signalR from '@microsoft/signalr';

interface PlayerListProps {
  title: string,
  playerScores: PlayerScore[],
  displayPoints: boolean,
  displayIndex: boolean,
  displayRound: boolean
}

function PlayerList({title, playerScores, displayPoints, displayIndex, displayRound}: PlayerListProps) {
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

    return () => {
      hub.off(HubEvents.onUpdateCurrentRound);
      hub.off(HubEvents.onUpdateDrawingPlayer);
    }
  }, [hub.getState()]);
  return (
    <>
      { displayRound && <h5>Round {gameState.currentRound}/{gameSettings.roundsCount}</h5>}
      <ul className="list-group">
        <li className="list-group-item justify-content-between align-items-center">
            <b>{title}</b>
        </li>
        {playerScores.map((playerScore, index) => (
          <li key={index} className="list-group-item d-flex justify-content-between align-items-center">
            { displayIndex && index + 1 + "."}
            <span className={player.username == playerScore.username ? "text-warning" : "text-dark"}>
              {playerScore.username}
              { gameState.hostPlayerUsername == playerScore.username && <BsShieldShaded/> }
              { gameState.drawingPlayerUsername == playerScore.username && <BsPaletteFill/> }
            </span>
            { displayPoints && <span className="badge rounded-pill bg-dark">{playerScore.score}</span> }
          </li>
          ))}
      </ul>
    </>
  )
}

export default PlayerList;