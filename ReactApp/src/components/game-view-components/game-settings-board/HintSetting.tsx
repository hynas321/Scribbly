import { useId } from "react";
import { BsLightbulbFill } from "react-icons/bs";
import config from "../../../../config.json";
import { useAppSelector } from "../../../redux/hooks";
import { HintSpeed, normalizeHintPercentage } from "../../../redux/slices/game-settings-slice";
import { TranslationKey, useTranslation } from "../../../i18n/I18nContext";
import SettingReadonlyLabel from "./SettingReadonlyLabel";

interface HintSettingProps {
  isPlayerHost: boolean;
  onSpeedChange: (value: HintSpeed) => void;
  onPercentageChange: (value: number) => void;
}

const speeds: Array<{ value: HintSpeed; label: TranslationKey }> = [
  { value: "off", label: "settings.hintsOff" },
  { value: "slow", label: "settings.hintSlow" },
  { value: "normal", label: "settings.hintNormal" },
  { value: "fast", label: "settings.hintFast" },
];

export default function HintSetting({
  isPlayerHost,
  onSpeedChange,
  onPercentageChange,
}: HintSettingProps) {
  const { t } = useTranslation();
  const id = useId();
  const { hintSpeed, hintPercentage } = useAppSelector((state) => state.gameSettings);
  const percentage = normalizeHintPercentage(hintPercentage);
  const percentageLabel = `${percentage}%`;
  const selectedSpeedLabel =
    speeds.find((speed) => speed.value === hintSpeed)?.label ?? "settings.hintsOff";

  return (
    <div className="mt-4">
      {isPlayerHost ? (
        <>
          <label htmlFor={`${id}-speed`} className="form-label d-block">
            {t("settings.hintSpeed")}
          </label>
          <select
            id={`${id}-speed`}
            className="form-select"
            value={hintSpeed}
            onChange={(event) => onSpeedChange(event.target.value as HintSpeed)}
          >
            {speeds.map((speed) => (
              <option key={speed.value} value={speed.value}>
                {t(speed.label)}
              </option>
            ))}
          </select>
          {hintSpeed !== "off" && (
            <div className="mt-4">
              <label htmlFor={`${id}-percentage`} className="form-label">
                {t("settings.hintPercentage")}: {percentageLabel}
              </label>
              <input
                id={`${id}-percentage`}
                type="range"
                className="form-range d-block mx-auto"
                style={{ maxWidth: "14rem", width: "100%" }}
                min={config.gameSettings.minHintPercentage}
                max={config.gameSettings.maxHintPercentage}
                step={config.gameSettings.hintPercentageStep}
                value={percentage}
                aria-valuemin={config.gameSettings.minHintPercentage}
                aria-valuemax={config.gameSettings.maxHintPercentage}
                aria-valuenow={percentage}
                aria-valuetext={percentageLabel}
                onChange={(event) => onPercentageChange(Number(event.target.value))}
              />
            </div>
          )}
        </>
      ) : (
        <>
          <SettingReadonlyLabel label={t("settings.hintSpeed")}>
            {t(selectedSpeedLabel)} <BsLightbulbFill />
          </SettingReadonlyLabel>
          {hintSpeed !== "off" && (
            <div className="mt-4">
              <SettingReadonlyLabel label={t("settings.hintPercentage")}>
                {percentageLabel}
              </SettingReadonlyLabel>
            </div>
          )}
        </>
      )}
    </div>
  );
}
