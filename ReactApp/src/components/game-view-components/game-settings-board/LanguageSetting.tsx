import LanguageSelect from "../../forms/LanguageSelect";
import { useAppSelector } from "../../../redux/hooks";
import { BsTranslate } from "react-icons/bs";
import { useTranslation } from "../../../i18n/I18nContext";
import SettingReadonlyLabel from "./SettingReadonlyLabel";

interface LanguageSettingProps {
  isPlayerHost: boolean;
  onChange: (value: string) => void;
}

const LanguageSetting = ({ isPlayerHost, onChange }: LanguageSettingProps) => {
  const { t } = useTranslation();
  const promptLanguage = useAppSelector((state) => state.gameSettings.promptLanguage);
  const languageLabel =
    promptLanguage === "en" ? t("settings.languageEnglish") : t("settings.languagePolish");

  return (
    <div className="mt-4">
      {isPlayerHost ? (
        <LanguageSelect
          title={t("settings.promptLanguage")}
          value={promptLanguage}
          onChange={onChange}
        />
      ) : (
        <SettingReadonlyLabel label={t("settings.language")}>
          {languageLabel} <BsTranslate />
        </SettingReadonlyLabel>
      )}
    </div>
  );
};

export default LanguageSetting;
