interface ProgressBarProps {
	currentProgress: number;
	minProgress: number;
	maxProgress: number;
	text?: string;
}

function ProgressBar({currentProgress, minProgress: minValue, maxProgress: maxValue, text}: ProgressBarProps) {
  return (
    <div className="progress" style={{height: "75%"}}>
      <div
        className="progress-bar"
        role="progressbar"
        style={{ width: `${currentProgress}%` }}
        aria-valuenow={currentProgress}
        aria-valuemin={minValue}
        aria-valuemax={maxValue}
      >
        {currentProgress}{text}
      </div>
    </div>
  );
}

export default ProgressBar;