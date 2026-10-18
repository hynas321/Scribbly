import config from "../../../../config.json";
import { useAppSelector } from "../../../redux/hooks";
import { useTranslation } from "../../../i18n/I18nContext";
import RangeInput from "../../forms/RangeInput";
import SettingReadonlyLabel from "./SettingReadonlyLabel";

interface DrawingTimeSettingProps {
  isPlayerHost: boolean;
  onChange: (value: number) => void;
}

const DrawingTimeSetting = ({ isPlayerHost, onChange }: DrawingTimeSettingProps) => {
  const { t } = useTranslation();
  const drawingTimeSeconds = useAppSelector((state) => state.gameSettings.drawingTimeSeconds);

  return (
    <div className="mt-4">
      {isPlayerHost ? (
        <RangeInput
          title={t("settings.drawingTime")}
          suffix={"s"}
          minValue={config.gameSettings.minDrawingTimeSeconds}
          maxValue={config.gameSettings.maxDrawingTimeSeconds}
          step={config.gameSettings.drawingTimeStepSeconds}
          value={drawingTimeSeconds}
          onChange={onChange}
        />
      ) : (
        <SettingReadonlyLabel label={t("settings.drawingTime")}>
          {drawingTimeSeconds}s
        </SettingReadonlyLabel>
      )}
    </div>
  );
};

export default DrawingTimeSetting;
