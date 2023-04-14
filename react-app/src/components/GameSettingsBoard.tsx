import Range from './Range';
import CheckForm from './CheckForm';
import CheckBox from './CheckBox';
import { useAppDispatch, useAppSelector } from '../redux/hooks';
import { updatedDrawingTimeSeconds, updatedNonAbstractNounsOnly, updatedRoundsCount } from '../redux/slices/game-settings-slice';
import { BsGearFill } from 'react-icons/bs';

function GameSettingsBoard() {
  const dispatch = useAppDispatch();

  const settings = {
    nonAbstractNounsOnly: useAppSelector((state) => state.gameSettings.nonAbstractNounsOnly),
    drawingTimespanSeconds: useAppSelector((state) => state.gameSettings.drawingTimeSeconds),
    roundsCount: useAppSelector((state) => state.gameSettings.roundsCount)
  };

  const handleCheckBoxChange = (checked: boolean) => {
    dispatch(updatedNonAbstractNounsOnly(checked));
  }

  const handleRangeChange = (value: number) => {
    dispatch(updatedDrawingTimeSeconds(value));
  }

  const handleCheckFormChange = (value: number) => {
    dispatch(updatedRoundsCount(value));
  }

  return (
    <div className="bg-light px-5 pt-3 pb-3">
      <h2 className="text-center">Game Settings <BsGearFill/></h2>
      <CheckBox
        text="Allow only non-abstract nouns"
        checkedByDefault={settings.nonAbstractNounsOnly}
        onChange={handleCheckBoxChange}
      />
      <Range
        title={"Drawing time"}
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