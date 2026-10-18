import config from "../../../../config.json";
import RadioGroup from "../../forms/RadioGroup";
import { useAppSelector } from "../../../redux/hooks";
import { BsLayersFill } from "react-icons/bs";
import { useTranslation } from "../../../i18n/I18nContext";
import SettingReadonlyLabel from "./SettingReadonlyLabel";

interface RoundsSettingProps {
  isPlayerHost: boolean;
  onChange: (value: number) => void;
}

const RoundsSetting = ({ isPlayerHost, onChange }: RoundsSettingProps) => {
  const { t } = useTranslation();
  const roundsCount = useAppSelector((state) => state.gameSettings.roundsCount);

  return (
    <div className="mt-4">
      {isPlayerHost ? (
        <RadioGroup
          title={t("settings.roundsCount")}
          minValue={config.gameSettings.minRounds}
          count={config.gameSettings.maxRounds - config.gameSettings.minRounds + 1}
          value={roundsCount}
          name="round-count"
          onChange={onChange}
        />
      ) : (
        <SettingReadonlyLabel label={t("settings.roundsCount")}>
          {roundsCount} <BsLayersFill />
        </SettingReadonlyLabel>
      )}
    </div>
  );
};

export default RoundsSetting;
