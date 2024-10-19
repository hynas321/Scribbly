import Range from "../Range";
import CheckForm from "../CheckForm";
import { useAppDispatch, useAppSelector } from "../../redux/hooks";
import {
  GameSettings,
  updatedDrawingTimeSeconds,
  updatedRoundsCount,
  updatedWordLanguage,
} from "../../redux/slices/game-settings-slice";
import { BsGearFill } from "react-icons/bs";
import InputSelect from "../InputSelect";
import { useContext, useEffect, useRef, useState } from "react";
import { ConnectionHubContext } from "../../context/ConnectionHubContext";
import * as signalR from "@microsoft/signalr";
import HubEvents from "../../hub/HubMessages";
import UrlHelper from "../../utils/UrlHelper";
import { animated, useSpring } from "@react-spring/web";
import { SessionStorageService } from "../../classes/SessionStorageService";

interface GameSettingsBoardProps {
  isPlayerHost: boolean;
}

function GameSettingsBoard({ isPlayerHost }: GameSettingsBoardProps) {
  const hub = useContext(ConnectionHubContext);
  const dispatch = useAppDispatch();
  const settings = useAppSelector((state) => state.gameSettings);

  const [gameHash, setGameHash] = useState<string>("");
  const firstRender = useRef(true);

  const sessionStorageService = SessionStorageService.getInstance();

  const gameSettingsBoardAnimationSpring = useSpring({
    from: { y: 200 },
    to: { y: 0 },
  });

  let gameSettingsLoaded = false;
  const drawingTimeText = "Drawing time";
  const numberOfRoundsText = "Number of rounds";
  const chooseLanguageText = "Language of random words";

  useEffect(() => {
    if (firstRender.current) {
      firstRender.current = false;
    }

    setGameHash(UrlHelper.getGameHash(window.location.href));
  }, []);

  useEffect(() => {
    if (hub.getState() != signalR.HubConnectionState.Connected || !gameHash) {
      return;
    }

    hub.on(HubEvents.onLoadGameSettings, (gameSettingsSerialized: any) => {
      const gameSettings = JSON.parse(gameSettingsSerialized) as GameSettings;

      dispatch(updatedDrawingTimeSeconds(gameSettings.drawingTimeSeconds));
      dispatch(updatedRoundsCount(gameSettings.roundsCount));
      dispatch(updatedWordLanguage(gameSettings.wordLanguage));
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
      const loadGameSettings = async () => {
        await hub.invoke(
          HubEvents.LoadGameSettings,
          gameHash,
          sessionStorageService.getAuthorizationToken()
        );

        gameSettingsLoaded = true;
      };

      loadGameSettings();
    }
    return () => {
      hub.off(HubEvents.onLoadGameSettings);
      hub.off(HubEvents.onSetDrawingTimeSeconds);
      hub.off(HubEvents.onSetRoundsCount);
      hub.off(HubEvents.onSetWordLanguage);
    };
  }, [hub.getState(), gameHash]);

  const handleRangeChange = async (value: number) => {
    if (!gameHash) {
      return;
    }

    await hub.invoke(
      HubEvents.setDrawingTimeSeconds,
      gameHash,
      sessionStorageService.getAuthorizationToken(),
      value
    );
  };

  const handleCheckFormChange = async (value: number) => {
    if (!gameHash) {
      return;
    }

    await hub.invoke(
      HubEvents.setRoundsCount,
      gameHash,
      sessionStorageService.getAuthorizationToken(),
      Number(value)
    );
  };

  const handleInputSelectChange = async (value: string) => {
    if (!gameHash) {
      return;
    }

    await hub.invoke(
      HubEvents.setWordLanguage,
      gameHash,
      sessionStorageService.getAuthorizationToken(),
      value
    );
  };

  return (
    <animated.div
      className="bg-light rounded-5 px-5 pt-3 pb-3"
      style={{ ...gameSettingsBoardAnimationSpring }}
    >
      <h4 className="text-center">
        Game Settings <BsGearFill />
      </h4>
      <div className="mt-4">
        {isPlayerHost ? (
          <Range
            title={drawingTimeText}
            suffix={"seconds"}
            minValue={30}
            maxValue={120}
            step={15}
            defaultValue={settings.drawingTimeSeconds}
            onChange={handleRangeChange}
          />
        ) : (
          <label className="form-check-label">
            {drawingTimeText}: <b>{settings.drawingTimeSeconds.valueOf().toString()}s</b>
          </label>
        )}
      </div>
      <div className="mt-4">
        {isPlayerHost ? (
          <CheckForm
            title={numberOfRoundsText}
            radioCount={6}
            defaultValue={settings.roundsCount}
            onChange={handleCheckFormChange}
          />
        ) : (
          <label className="form-check-label">
            {numberOfRoundsText}: <b>{settings.roundsCount.valueOf().toString()}</b>
          </label>
        )}
      </div>
      <div className="mt-4">
        {isPlayerHost ? (
          <InputSelect
            title={chooseLanguageText}
            defaultValue={settings.wordLanguage}
            onChange={handleInputSelectChange}
          />
        ) : (
          <label className="form-check-label">
            {chooseLanguageText}:
            <b>
              {settings.wordLanguage.valueOf().toString() === "en"
                ? " English"
                : settings.wordLanguage.valueOf().toString() === "pl"
                ? " Polish"
                : "?"}
            </b>
          </label>
        )}
      </div>
    </animated.div>
  );
}

export default GameSettingsBoard;
