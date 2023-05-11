import { Link } from "react-router-dom";
import Button from "../Button";
import config from "../../../config.json"
import PlayerList from "../game-view-components/PlayerList";

function ResultsView() {
  return (
    <div className="container text-center mt-3">
    <h3 className="mt-3">Game finished</h3>
    <div>
        <PlayerList
            playerScores={[]}
            title={"Scoreboard"}
            displayPoints={false}
            displayIndex={false}
            displayRound={false}        
        />
    </div>
    <div>
      <Link to={config.mainClientEndpoint}>
        <Button 
          text={"Go to main page"}
          active={true}
          type="success"
        />
        </Link>
    </div>
  </div>
  )
}

export default ResultsView;