import ClipboardBar from "../../bars/ClipboardBar";
import Button from "../../Button";
import { BsPlayCircle, BsDoorOpen } from "react-icons/bs";
import { useAppSelector } from "../../../redux/hooks";
import PlayerScores from "../../game-view-components/player-score/PlayerScores";
import GameSettingsBoard from "../../game-view-components/game-settings-board/GameSettingsBoard";
import Chat from "../../game-view-components/chat/Chat";
import { useLobbyActions } from "../../../hooks/useLobbyActions";
import { useContext } from "react";
import { ConnectionHubContext, LongRunningConnectionHubContext } from "../../../context/ConnectionHubContext";

interface LobbyViewProps {
  playerScores: any;
  isPlayerHost: boolean;
  isStartGameButtonActive: boolean;
  onLeave: () => void;
  gameHash: string;
}

const InLobby = ({
  playerScores,
  isPlayerHost,
  isStartGameButtonActive,
  onLeave,
  gameHash
}: LobbyViewProps) => {
  const hub = useContext(ConnectionHubContext);
  const longRunningHub = useContext(LongRunningConnectionHubContext);
  const player = useAppSelector(state => state.player);
  const gameSettings = useAppSelector(state => state.gameSettings);
  const { handleStartGame } = useLobbyActions(hub, longRunningHub, gameHash, gameSettings);

  return (
    <div className="container mb-3">
      <div className="row">
        <div className="col-lg-4 col-sm-5 col-12 mx-auto mt-2 text-center order-lg-1 order-2 mb-3">
          <div className="col-lg-6">
            <PlayerScores
              title={"Players in the lobby"}
              playerScores={playerScores}
              displayPoints={false}
              displayIndex={false}
              displayRound={false}
            />
            <ClipboardBar invitationUrl={window.location.href} />
          </div>
        </div>
        <div className="col-lg-4 col-sm-10 col-12 mx-auto text-center order-lg-2 order-1">
          <h5>Your username: {player.username}</h5>
          {isPlayerHost ? (
            <Button
              text="Start the game"
              type="success"
              active={isStartGameButtonActive}
              icon={<BsPlayCircle />}
              onClick={handleStartGame}
            />
          ) : (
            <h4 className="mt-3">Waiting for the host to start the game</h4>
          )}
          <Button
            text="Leave the game"
            active={true}
            icon={<BsDoorOpen />}
            type={"danger"}
            onClick={onLeave}
          />
          <div className="mt-3">
            <GameSettingsBoard isPlayerHost={isPlayerHost} />
          </div>
        </div>
        <div className="col-lg-4 order-lg-3 col-sm-7 col-12 order-3">
          <div className="col-lg-9 col-sm-12 col-12 float-end mb-3">
            <Chat
              placeholderValue={"Enter your message"}
              displaySecretWord={false}
            />
          </div>
        </div>
      </div>
    </div>
  );
};

export default InLobby;
