import Range from './Range';
import CheckForm from './CheckForm';
import CheckBox from './CheckBox';
import Settings from '../interfaces/Settings';

interface GameSettingsBoardProps {
  defaultSettings: Settings;
  onChange: (settings: Settings) => void;
};

function GameSettingsBoard({onChange, defaultSettings}: GameSettingsBoardProps) {
  let settings = defaultSettings;

  const handleCheckBoxChange = (checked: boolean) => {
    settings.nonAbstractNounsChecked = checked;
    onChange(settings);
  }

  const handleRangeChange = (value: number) => {
    settings.drawingTimespanSeconds = value;
    onChange(settings);
  }

  const handleCheckFormChange = (value: number) => {
    settings.roundsCount = value;
    onChange(settings);
  }

  return (
    <div className="bg-light px-5 pt-3 pb-3">
      <h2 className="text-center">Game Settings</h2>
      <CheckBox
        checkedByDefault={settings.nonAbstractNounsChecked}
        onChange={handleCheckBoxChange}
      />
      <Range
        title={`Drawing timespan`}
        minValue={30}
        maxValue={120}
        step={15}
        defaultValue={settings.drawingTimespanSeconds}
        onChange={handleRangeChange} 
      />
      <CheckForm
        title={"Number of rounds"}
        radioCount={6}
        radioCheckedByDefault={settings.roundsCount}
        onChange={handleCheckFormChange}
      />
    </div>
  );
}

export default GameSettingsBoard;