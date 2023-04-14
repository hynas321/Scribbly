import Range from './Range';
import CheckForm from './CheckForm';
import CheckBox from './CheckBox';
import { useAppDispatch, useAppSelector } from '../redux/hooks';
import { updatedDrawingTimeSeconds, updatedNonAbstractNounsOnly, updatedRoundsCount, updatedWordLanguage } from '../redux/slices/game-settings-slice';
import { BsGearFill } from 'react-icons/bs';
import InputSelect from './InputSelect';

function GameSettingsBoard() {
  const dispatch = useAppDispatch();

  const settings = {
    nonAbstractNounsOnly: useAppSelector((state) => state.gameSettings.nonAbstractNounsOnly),
    drawingTimespanSeconds: useAppSelector((state) => state.gameSettings.drawingTimeSeconds),
    roundsCount: useAppSelector((state) => state.gameSettings.roundsCount),
    wordLanguage: useAppSelector((state) => state.gameSettings.wordLanguage)
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

  const handleInputSelectChange = (value: string) => {
    dispatch(updatedWordLanguage(value));
  }

  return (
    <div className="bg-light px-5 pt-3 pb-3">
      <h4 className="text-center">Game Settings <BsGearFill/></h4>
      <div className="mt-4">
        <CheckBox
          text="Allow only non-abstract nouns"
          defaultValue={settings.nonAbstractNounsOnly}
          onChange={handleCheckBoxChange}
        />
      </div>
      <div className="mt-4">
        <Range
          title={"Drawing time"}
          minValue={30}
          maxValue={120}
          step={15}
          defaultValue={settings.drawingTimespanSeconds}
          onChange={handleRangeChange} 
        />
      </div>
      <div className="mt-4">
        <CheckForm
          title={"Number of rounds"}
          radioCount={6}
          defaultValue={settings.roundsCount}
          onChange={handleCheckFormChange}
        />
      </div>
      <div className="mt-4">
        <InputSelect
          title={"Choose the language of random words"}
          defaultValue={settings.wordLanguage}
          onChange={handleInputSelectChange}
        />
      </div>
    </div>
  );
}

export default GameSettingsBoard;