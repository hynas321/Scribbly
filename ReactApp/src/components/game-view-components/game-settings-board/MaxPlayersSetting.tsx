import config from "../../../../config.json";
import { useAppSelector } from "../../../redux/hooks";
import { useTranslation } from "../../../i18n/I18nContext";
import { BsPeopleFill } from "react-icons/bs";
import RangeInput from "../../forms/RangeInput";
import SettingReadonlyLabel from "./SettingReadonlyLabel";

interface MaxPlayersSettingProps {
  isPlayerHost: boolean;
  onChange: (value: number) => void;
}

const MaxPlayersSetting = ({ isPlayerHost, onChange }: MaxPlayersSettingProps) => {
  const { t } = useTranslation();
  const maxPlayers = useAppSelector((state) => state.gameSettings.maxPlayers);

  return (
    <div className="mt-4">
      {isPlayerHost ? (
        <RangeInput
          title={t("settings.maxPlayers")}
          suffix={""}
          minValue={config.gameSettings.minPlayers}
          maxValue={config.gameSettings.maxPlayers}
          step={1}
          value={maxPlayers}
          onChange={onChange}
        />
      ) : (
        <SettingReadonlyLabel label={t("settings.maxPlayers")}>
          {maxPlayers} <BsPeopleFill />
        </SettingReadonlyLabel>
      )}
    </div>
  );
};

export default MaxPlayersSetting;
