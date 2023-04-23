import Range from './Range';
import CheckForm from './CheckForm';
import CheckBox from './CheckBox';
import { useAppDispatch, useAppSelector } from '../redux/hooks';
import { GameSettings, updatedDrawingTimeSeconds, updatedNonAbstractNounsOnly, updatedRoundsCount, updatedWordLanguage } from '../redux/slices/game-settings-slice';
import { BsGearFill } from 'react-icons/bs';
import InputSelect from './InputSelect';
import { useContext, useEffect } from "react";
import { LobbyHubContext } from '../Context/LobbyHubContext';
import * as signalR from '@microsoft/signalr';

interface GameSettingsBoardProps {
  isPlayerHost: boolean;
}

function GameSettingsBoard({isPlayerHost}: GameSettingsBoardProps) {
  const hub = useContext(LobbyHubContext);
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

    hub.on("ApplyGameSettings", (gameSettingsSerialized: any) => {
      const gameSettings = JSON.parse(gameSettingsSerialized) as GameSettings;
      dispatch(updatedNonAbstractNounsOnly(gameSettings.nonAbstractNounsOnly));
      dispatch(updatedDrawingTimeSeconds(gameSettings.drawingTimeSeconds));
      dispatch(updatedRoundsCount(gameSettings.roundsCount));
      dispatch(updatedWordLanguage(gameSettings.wordLanguage));
    });

    hub.on("ApplyAbstractNounsSetting", (checked: boolean) => {
      console.log("Test")
      dispatch(updatedNonAbstractNounsOnly(checked));
    });

    hub.on("ApplyDrawingTimeSetting", (value: number) => {
      dispatch(updatedDrawingTimeSeconds(value));
    });

    hub.on("ApplyRoundsCountSetting", (value: number) => {
      dispatch(updatedRoundsCount(value));
    });

    hub.on("ApplyWordLanguageSetting", (value: string) => {
      dispatch(updatedWordLanguage(value));
    });

    if (!gameSettingsLoaded) {
      const getGameSettings = async() => {
        await hub.invoke("GetGameSettings", testLobbyHash);
        gameSettingsLoaded = true;
      }

      getGameSettings();
    }
    
    return () => {
      hub.off("ApplyGameSettings");
      hub.off("ApplyAbstractNounsSetting");
      hub.off("ApplyDrawingTimeSetting");
      hub.off("ApplyRoundsCountSetting");
      hub.off("ApplyWordLanguageSetting");
    }
  }, [hub.getState()])

  const handleCheckBoxChange = async (checked: boolean) => {
    await hub.invoke("ChangeAbstractNounsSetting", testLobbyHash, checked);
  }

  const handleRangeChange = async (value: number) => {
    await hub.invoke("ChangeDrawingTimeSetting", testLobbyHash, value);
  }

  const handleCheckFormChange = async (value: number) => {
    await hub.invoke("ChangeRoundsCountSetting", testLobbyHash, Number(value));
  }

  const handleInputSelectChange = async (value: string) => {
    await hub.invoke("ChangeWordLanguageSetting", testLobbyHash, value);
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