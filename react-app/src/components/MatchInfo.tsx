import ProgressBar from "./ProgressBar";

interface MatchInfo {
  currentRound: number;
  roundCount: number;
  currentProgress: number;
  minProgress: number;
  maxProgress: number;
}

function MatchInfo({currentRound, roundCount, currentProgress, minProgress, maxProgress}: MatchInfo) {
  return (
    <div className="row d-flex justify-content-between">
      <div className="col-3">
        <h5>Round {currentRound}/{roundCount}</h5>
      </div>
      <div className="col-9">
        <ProgressBar
          currentProgress={currentProgress}
          minProgress={minProgress}
          maxProgress={maxProgress}
          text="s"
        />
      </div>

    </div>
  )
}

export default MatchInfo;