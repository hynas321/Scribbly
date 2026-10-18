import { ChangeEvent, useId } from "react";
import { useTranslation } from "../../i18n/I18nContext";

interface LanguageSelectProps {
  title: string;
  value: string;
  onChange: (value: string) => void;
}

function LanguageSelect({ title, value, onChange }: LanguageSelectProps) {
  const { t } = useTranslation();
  const id = useId();
  const handleChange = (event: ChangeEvent<HTMLSelectElement>) => onChange(event.target.value);
  return (
    <>
      <label className="form-label d-block" htmlFor={id}>
        {title}
      </label>
      <select id={id} className="form-select" value={value} onChange={handleChange}>
        <option value="en">{t("settings.languageEnglish")}</option>
        <option value="pl">{t("settings.languagePolish")}</option>
      </select>
    </>
  );
}

export default LanguageSelect;
