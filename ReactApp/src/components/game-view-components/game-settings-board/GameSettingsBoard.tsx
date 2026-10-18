import { BsGearFill } from "react-icons/bs";
import { useContext } from "react";
import { animated, useSpring } from "@react-spring/web";
import { useParams } from "react-router-dom";
import { ConnectionHubContext } from "../../../context/ConnectionHubContext";
import HubEvents from "../../../hub/HubMessages";
import { reportRealtimeError } from "../../../utils/reportRealtimeError";
import RoundsSetting from "./RoundSetting";
import LanguageSetting from "./LanguageSetting";
import DrawingTimeSetting from "./DrawingTimeSetting";
import MaxPlayersSetting from "./MaxPlayersSetting";
import HintSetting from "./HintSetting";
import CategorySetting from "./CategorySetting";
import { useGameSettingsRealtime } from "../../../features/game-settings/useGameSettingsRealtime";
import { useTranslation } from "../../../i18n/I18nContext";
import "./GameSettingsBoard.css";

interface GameSettingsBoardProps {
  isPlayerHost: boolean;
}

const GameSettingsBoard = ({ isPlayerHost }: GameSettingsBoardProps) => {
  const { t } = useTranslation();
  const hub = useContext(ConnectionHubContext);
  const { gameHash = "" } = useParams();
  useGameSettingsRealtime(hub);
  const animation = useSpring({ from: { y: 200 }, to: { y: 0 } });

  const change = async (method: string, value: number | string) => {
    if (!isPlayerHost || !gameHash) {
      return;
    }
    await hub
      .invoke(method, gameHash, value)
      .catch(() => reportRealtimeError(t("error.action.changeSettings")));
  };

  return (
    <animated.div className="game-settings-board bg-light" style={{ ...animation }}>
      <h4 className="text-center">
        {t("settings.title")} <BsGearFill />
      </h4>
      <DrawingTimeSetting
        isPlayerHost={isPlayerHost}
        onChange={(value) => void change(HubEvents.setDrawingTimeSeconds, value)}
      />
      <RoundsSetting
        isPlayerHost={isPlayerHost}
        onChange={(value) => void change(HubEvents.setRoundsCount, value)}
      />
      <LanguageSetting
        isPlayerHost={isPlayerHost}
        onChange={(value) => void change(HubEvents.setPromptLanguage, value)}
      />
      <MaxPlayersSetting
        isPlayerHost={isPlayerHost}
        onChange={(value) => void change(HubEvents.setMaxPlayers, value)}
      />
      <HintSetting
        isPlayerHost={isPlayerHost}
        onSpeedChange={(value) => void change(HubEvents.setHintSpeed, value)}
        onPercentageChange={(value) => void change(HubEvents.setHintPercentage, value)}
      />
      <CategorySetting isPlayerHost={isPlayerHost} />
    </animated.div>
  );
};
export default GameSettingsBoard;
