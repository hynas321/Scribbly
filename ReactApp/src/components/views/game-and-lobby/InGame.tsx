import Canvas from "../../game-view-components/canvas/Canvas";
import Chat from "../../game-view-components/chat/Chat";
import ControlPanel from "../../game-view-components/control-panel/ControlPanel";
import PlayerScores from "../../game-view-components/player-score/PlayerScores";

interface InGameViewProps {
  playerScores: any;
  onLeave: () => void;
}

const InGame = ({ playerScores, onLeave }: InGameViewProps) => {
  return (
    <div className="container text-center">
      <div className="row">
        <div className="col-xl-2 col-lg-6 col-md-6 col-12 order-xl-1 order-lg-2 order-md-2 order-3 mb-3">
          <PlayerScores
            title={"Scoreboard"}
            playerScores={playerScores}
            displayPoints={true}
            displayIndex={true}
            displayRound={true}
          />
          <ControlPanel onClick={onLeave} />
        </div>
        <div className="col-xl-7 col-lg-12 col-md-12 col-12 order-xl-2 order-lg-1 order-md-1 order-1 mb-3">
          <Canvas />
        </div>
        <div className="col-xl-3 col-lg-6 col-md-6 col-12 order-xl-2 order-lg-3 order-md-3 order-2 mb-3">
          <Chat
            placeholderValue="Enter your guess"
            displaySecretWord={true}
          />
        </div>
      </div>
    </div>
  );
};

export default InGame;
