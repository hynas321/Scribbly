import { BsGearFill } from "react-icons/bs";
import { useContext, useEffect, useRef, useState } from "react";
import { ConnectionHubContext } from "../../../context/ConnectionHubContext";
import HubEvents from "../../../hub/HubMessages";
import UrlHelper from "../../../utils/UrlHelper";
import { animated, useSpring } from "@react-spring/web";
import { useSessionStorage } from "../../../hooks/useSessionStorage";
import RoundsSetting from "./RoundSetting";
import LanguageSetting from "./LanguageSetting";
import DrawingTimeSetting from "./DrawingTimeSetting";
import { useGameSettingsBoardHubEvents } from "../../../hooks/hub-events/useGameSettingsBoardHubEvents";

interface GameSettingsBoardProps {
  isPlayerHost: boolean;
}

const GameSettingsBoard = ({ isPlayerHost }: GameSettingsBoardProps) => {
  const hub = useContext(ConnectionHubContext);
  const firstRender = useRef(true);
  const { authorizationToken } = useSessionStorage();

  const [gameHash, setGameHash] = useState<string>("");

  const gameSettingsBoardAnimationSpring = useSpring({
    from: { y: 200 },
    to: { y: 0 },
  });

  useEffect(() => {
    if (firstRender.current) {
      firstRender.current = false;
    }

    setGameHash(UrlHelper.getGameHash(window.location.href));
  }, []);

  useGameSettingsBoardHubEvents(hub, gameHash);

  const handleDrawingTimeChange = async (value: number) => {
    if (!gameHash) return;
    await hub.invoke(HubEvents.setDrawingTimeSeconds, gameHash, authorizationToken, value);
  };

  const handleRoundsChange = async (value: number) => {
    if (!gameHash) return;
    await hub.invoke(HubEvents.setRoundsCount, gameHash, authorizationToken, Number(value));
  };

  const handleLanguageChange = async (value: string) => {
    if (!gameHash) return;
    await hub.invoke(HubEvents.setWordLanguage, gameHash, authorizationToken, value);
  };

  return (
    <animated.div
      className="bg-light rounded-5 px-5 pt-3 pb-3"
      style={{ ...gameSettingsBoardAnimationSpring }}
    >
      <h4 className="text-center">
        Game Settings <BsGearFill />
      </h4>
      <DrawingTimeSetting
        isPlayerHost={isPlayerHost}
        onChange={handleDrawingTimeChange}
      />
      <RoundsSetting
        isPlayerHost={isPlayerHost}
        onChange={handleRoundsChange}
      />
      <LanguageSetting
        isPlayerHost={isPlayerHost}
        onChange={handleLanguageChange} />
    </animated.div>
  );
}

export default GameSettingsBoard;
