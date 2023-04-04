import { useState } from 'react';
import Range from './Range';
import CheckBox from './Checkbox';
import CheckForm from './CheckForm';

function GameSettingsBoard() {
  const [roundsCount, setRoundsCount] = useState(1);
  const [nonAbstractNounsChecked, setNonAbstractNounsChecked] = useState(false);

  const handleInputRangeChange = (value: number) => {
    setRoundsCount(value);
  }

  const handleCheckBoxChange = (checked: boolean) => {
    setNonAbstractNounsChecked(checked);
  }

  return (
    <div className="bg-light px-5 pt-3 pb-3">
      <h2 className="text-center">Game Settings</h2>
      <CheckBox
        checkedByDefault={true}
        onChange={handleCheckBoxChange}
      />
      <Range
        title={"Number of rounds: " + roundsCount}
        minValue={1}
        maxValue={6}
        onChange={handleInputRangeChange} 
      />
      <CheckForm />
    </div>
  );
}

export default GameSettingsBoard;