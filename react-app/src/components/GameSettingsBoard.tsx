import Range from './Range';
import CheckForm from './CheckForm';
import CheckBox from './CheckBox';
import { useAppDispatch, useAppSelector } from '../redux/hooks';
import { updatedDrawingTimeSeconds, updatedNonAbstractNounsOnly, updatedRoundsCount, updatedWordLanguage } from '../redux/slices/game-settings-slice';
import { BsGearFill } from 'react-icons/bs';
import InputSelect from './InputSelect';

interface GameSettingsBoardProps {
  isPlayerHost: boolean;
}

function GameSettingsBoard({isPlayerHost}: GameSettingsBoardProps) {
  const dispatch = useAppDispatch();

  const nonAbstractNounsOnlyText = "Allow only non-abstract nouns";
  const drawingTimeText = "Drawing time";
  const numberOfRoundsText = "Number of rounds";
  const chooseLanguageText = "Language of random words";

  const settings = {
    nonAbstractNounsOnly: useAppSelector((state) => state.gameSettings.nonAbstractNounsOnly),
    drawingTimeSeconds: useAppSelector((state) => state.gameSettings.drawingTimeSeconds),
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
        { isPlayerHost ?
          <CheckBox
            text={nonAbstractNounsOnlyText}
            defaultValue={settings.nonAbstractNounsOnly}
            onChange={handleCheckBoxChange}
          /> :
          <label className="form-check-label">
            {nonAbstractNounsOnlyText}: <b>{settings.nonAbstractNounsOnly.valueOf().toString()}</b>
          </label>
        }
      </div>
      <div className="mt-4">
        { isPlayerHost ?
          <Range
            title={drawingTimeText}
            minValue={30}
            maxValue={120}
            step={15}
            defaultValue={settings.drawingTimeSeconds}
            onChange={handleRangeChange} 
          /> :
          <label className="form-check-label">
            {drawingTimeText}:<b>{settings.drawingTimeSeconds.valueOf().toString()}s</b>
          </label>
        }
      </div>
      <div className="mt-4">
        { isPlayerHost ?
          <CheckForm
            title={numberOfRoundsText}
            radioCount={6}
            defaultValue={settings.roundsCount}
            onChange={handleCheckFormChange}
          /> :
          <label className="form-check-label">
            {numberOfRoundsText}: <b>{settings.roundsCount.valueOf().toString()}</b>
          </label>
        }
      </div>
      <div className="mt-4">
        { isPlayerHost ?
          <InputSelect
            title={chooseLanguageText}
            defaultValue={settings.wordLanguage}
            onChange={handleInputSelectChange}
          /> :
          <label className="form-check-label">
            {chooseLanguageText}: <b>{settings.wordLanguage.valueOf().toString()}</b>
        </label>
        }
      </div>
    </div>
  );
}

export default GameSettingsBoard;