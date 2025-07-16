import InputSelect from "../../InputSelect";
import { useAppSelector } from "../../../redux/hooks";
import { BsTranslate } from "react-icons/bs";

interface LanguageSettingProps {
  isPlayerHost: boolean;
  onChange: (value: string) => void;
}

const LanguageSetting = ({ isPlayerHost, onChange }: LanguageSettingProps) => {
  const wordLanguage = useAppSelector((state) => state.gameSettings.wordLanguage);

  return (
    <div className="mt-4">
      {isPlayerHost ? (
        <InputSelect
          title={"Language of random words"}
          defaultValue={wordLanguage}
          onChange={onChange}
        />
      ) : (
        <label className="form-check-label">
          Language: <b>{wordLanguage === "en" ? "English" : "Polish"}</b> <BsTranslate />
        </label>
      )}
    </div>
  );
}

export default LanguageSetting;