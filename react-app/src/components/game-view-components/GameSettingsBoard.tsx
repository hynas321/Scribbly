import Range from '../Range';
import CheckForm from '../CheckForm';
import CheckBox from '../CheckBox';
import { useAppDispatch, useAppSelector } from '../../redux/hooks';
import { GameSettings, updatedDrawingTimeSeconds, updatedNounsOnly, updatedRoundsCount, updatedWordLanguage } from '../../redux/slices/game-settings-slice';
import { BsGearFill } from 'react-icons/bs';
import InputSelect from '../InputSelect';
import { useContext, useEffect, useState } from "react";
import { ConnectionHubContext } from '../../context/ConnectionHubContext';
import * as signalR from '@microsoft/signalr';
import HubEvents from '../../hub/HubEvents';
import useLocalStorageState from 'use-local-storage-state';
import UrlHelper from '../../utils/UrlHelper';

interface GameSettingsBoardProps {
  isPlayerHost: boolean;
}

function GameSettingsBoard({isPlayerHost}: GameSettingsBoardProps) {
  const hub = useContext(ConnectionHubContext);
  const dispatch = useAppDispatch();
  const settings = useAppSelector((state) => state.gameSettings);

  const [gameHash, setGameHash] = useState<string>("");

  const [token, setToken] = useLocalStorageState("token", { defaultValue: "" });

  let gameSettingsLoaded = false;
  const nounsOnly = "Allow only nouns as random words";
  const drawingTimeText = "Drawing time";
  const numberOfRoundsText = "Number of rounds";
  const chooseLanguageText = "Language of random words";

  
  useEffect(() => {
    setGameHash(UrlHelper.getGameHash(window.location.href));
  }, []);

  useEffect(() => {
    if (hub.getState() != signalR.HubConnectionState.Connected) {
      return;
    }

    hub.on(HubEvents.onLoadGameSettings, (gameSettingsSerialized: any) => {
      const gameSettings = JSON.parse(gameSettingsSerialized) as GameSettings;

      dispatch(updatedNounsOnly(gameSettings.nounsOnly));
      dispatch(updatedDrawingTimeSeconds(gameSettings.drawingTimeSeconds));
      dispatch(updatedRoundsCount(gameSettings.roundsCount));
      dispatch(updatedWordLanguage(gameSettings.wordLanguage));
    });

    hub.on(HubEvents.onSetAbstractNouns, (checked: boolean) => {
      dispatch(updatedNounsOnly(checked));
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
        await hub.invoke(HubEvents.LoadGameSettings, gameHash, token);

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
  }, [hub.getState(), gameHash])

  const handleCheckBoxChange = async (checked: boolean) => {
    await hub.invoke(HubEvents.setAbstractNouns, gameHash, token, checked);
  }

  const handleRangeChange = async (value: number) => {
    await hub.invoke(HubEvents.setDrawingTimeSeconds, gameHash, token, value);
  }

  const handleCheckFormChange = async (value: number) => {
    await hub.invoke(HubEvents.setRoundsCount, gameHash, token, Number(value));
  }

  const handleInputSelectChange = async (value: string) => {
    await hub.invoke(HubEvents.setWordLanguage, gameHash, token, value);
  }

  return (
    <div className="bg-light rounded-5 px-5 pt-3 pb-3">
      <h4 className="text-center">Game Settings <BsGearFill/></h4>
      <div className="mt-4">
        { isPlayerHost ?
          <CheckBox
            text={nounsOnly}
            defaultValue={settings.nounsOnly}
            onChange={handleCheckBoxChange}
          /> :
          <label className="form-check-label">
            {nounsOnly}: <b>{settings.nounsOnly.valueOf().toString() ? "Yes" : "No"}</b>
          </label>
        }
      </div>
      <div className="mt-4">
        { isPlayerHost ?
          <Range
            title={drawingTimeText}
            suffix={"seconds"}
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
            {chooseLanguageText}: 
            <b>
              {
                settings.wordLanguage.valueOf().toString() == "en" ? " English" :
                settings.wordLanguage.valueOf().toString() == "pl" ? " Polish" : "?"
              }
            </b>
        </label>
        }
      </div>
    </div>
  );
}

export default GameSettingsBoard;