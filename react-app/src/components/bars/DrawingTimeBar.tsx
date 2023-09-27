interface ProgressBarProps {
  currentProgress: number;
  minProgress: number;
  maxProgress: number;
	text?: string;
}

function DrawingTimeBar({currentProgress, minProgress, maxProgress, text}: ProgressBarProps) {
  return (
    <div className="progress" style={{height: "110%"}}>
      <div
        className="progress-bar progress-bar-striped progress-bar-animated"
        role="progressbar"
        style={{ width: `${(currentProgress / maxProgress) * 100}%`}}
        aria-valuenow={currentProgress}
        aria-valuemin={minProgress}
        aria-valuemax={maxProgress}
      >
        {currentProgress}{text}
      </div>
    </div>
  );
}

export default DrawingTimeBar;