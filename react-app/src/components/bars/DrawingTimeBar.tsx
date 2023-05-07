interface ProgressBarProps {
	progressProperties: ProgressProperties
	text?: string;
}

function DrawingTimeBar({progressProperties, text}: ProgressBarProps) {
  return (
    <div className="progress" style={{height: "110%"}}>
      <div
        className="progress-bar"
        role="progressbar"
        style={{ width: `${(progressProperties.currentProgress / progressProperties.maxProgress) * 100}%`}}
        aria-valuenow={progressProperties.currentProgress}
        aria-valuemin={progressProperties.minProgress}
        aria-valuemax={progressProperties.maxProgress}
      >
        {progressProperties.currentProgress}{text}
      </div>
    </div>
  );
}

export default DrawingTimeBar;