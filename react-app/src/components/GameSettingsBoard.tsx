import Range from './Range';
import CheckForm from './CheckForm';
import CheckBox from './CheckBox';
import { useAppDispatch, useAppSelector } from '../redux/hooks';
import { GameSettings, updatedDrawingTimeSeconds, updatedNonAbstractNounsOnly, updatedRoundsCount, updatedWordLanguage } from '../redux/slices/game-settings-slice';
import { BsGearFill } from 'react-icons/bs';
import InputSelect from './InputSelect';
import { useContext, useEffect } from "react";
import { ConnectionHubContext, connectionHub } from '../context/ConnectionHubContext';
import * as signalR from '@microsoft/signalr';
import HubEvents from '../hub/HubEvents';

interface GameSettingsBoardProps {
  isPlayerHost: boolean;
}

function GameSettingsBoard({isPlayerHost}: GameSettingsBoardProps) {
  const hub = useContext(ConnectionHubContext);
  const player = useAppSelector((state) => state.player);
  const dispatch = useAppDispatch();
  let gameSettingsLoaded = false;

  const nonAbstractNounsOnlyText = "Allow only non-abstract nouns";
  const drawingTimeText = "Drawing time";
  const numberOfRoundsText = "Number of rounds";
  const chooseLanguageText = "Language of random words";
  const testLobbyHash = "TestLobbyHash"; //temporary

  const settings = {
    nonAbstractNounsOnly: useAppSelector((state) => state.gameSettings.nonAbstractNounsOnly),
    drawingTimeSeconds: useAppSelector((state) => state.gameSettings.drawingTimeSeconds),
    roundsCount: useAppSelector((state) => state.gameSettings.roundsCount),
    wordLanguage: useAppSelector((state) => state.gameSettings.wordLanguage)
  };

  useEffect(() => {
    if (hub.getState() != signalR.HubConnectionState.Connected) {
      return;
    }

    hub.on(HubEvents.onLoadGameSettings, (gameSettingsSerialized: any) => {
      const gameSettings = JSON.parse(gameSettingsSerialized) as GameSettings;
      dispatch(updatedNonAbstractNounsOnly(gameSettings.nonAbstractNounsOnly));
      dispatch(updatedDrawingTimeSeconds(gameSettings.drawingTimeSeconds));
      dispatch(updatedRoundsCount(gameSettings.roundsCount));
      dispatch(updatedWordLanguage(gameSettings.wordLanguage));
    });

    hub.on(HubEvents.onSetAbstractNouns, (checked: boolean) => {
      dispatch(updatedNonAbstractNounsOnly(checked));
    });

    hub.on(HubEvents.onSetDrawingTimeSeconds, (value: number) => {
      dispatch(updatedDrawingTimeSeconds(value));
    });

    hub.on(HubEvents.onSetRoundsCount, (value: number) => {
      dispatch(updatedRoundsCount(value));
    });

    hub.on(HubEvents.onSetWordLanguage, (value: string) => {
      dispatch(updatedWordLanguage(value));
    });

    if (!gameSettingsLoaded) {
      const loadGameSettings = async() => {
        //await hub.invoke(HubEvents.LoadGameSettings);

        gameSettingsLoaded = true;
      }

      loadGameSettings();
    }
    return () => {
      hub.off(HubEvents.onLoadGameSettings);
      hub.off(HubEvents.onSetAbstractNouns);
      hub.off(HubEvents.onSetDrawingTimeSeconds);
      hub.off(HubEvents.onSetRoundsCount);
      hub.off(HubEvents.onSetWordLanguage);
    }
  }, [hub.getState()])

  const handleCheckBoxChange = async (checked: boolean) => {
    //await hub.invoke(HubEvents.setAbstractNouns, testLobbyHash, checked);
  }

  const handleRangeChange = async (value: number) => {
    //await hub.invoke(HubEvents.setDrawingTimeSeconds, testLobbyHash, value);
  }

  const handleCheckFormChange = async (value: number) => {
    //await hub.invoke(HubEvents.setRoundsCount, testLobbyHash, Number(value));
  }

  const handleInputSelectChange = async (value: string) => {
    //await hub.invoke(HubEvents.setWordLanguage, testLobbyHash, value);
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