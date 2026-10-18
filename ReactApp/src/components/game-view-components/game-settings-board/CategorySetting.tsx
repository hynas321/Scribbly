import { useContext, useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { ConnectionHubContext } from "../../../context/ConnectionHubContext";
import HubEvents from "../../../hub/HubMessages";
import { reportRealtimeError } from "../../../utils/reportRealtimeError";
import { useAppSelector } from "../../../redux/hooks";
import api from "../../../http/Api";
import { translateCategory, useTranslation } from "../../../i18n/I18nContext";

interface CategorySettingProps {
  isPlayerHost: boolean;
}

const CategorySetting = ({ isPlayerHost }: CategorySettingProps) => {
  const { t } = useTranslation();
  const hub = useContext(ConnectionHubContext);
  const { gameHash = "" } = useParams();
  const promptLanguage = useAppSelector((state) => state.gameSettings.promptLanguage);
  const promptCategories = useAppSelector((state) => state.gameSettings.promptCategories);
  const [categories, setCategories] = useState<string[]>([]);

  useEffect(() => {
    let active = true;
    api
      .fetchPromptCategories(promptLanguage)
      .then((items) => {
        if (active) {
          setCategories(items);
        }
      })
      .catch(() => {
        if (active) {
          setCategories([]);
        }
      });
    return () => {
      active = false;
    };
  }, [promptLanguage]);

  const isChecked = (slug: string) =>
    promptCategories.length === 0 || promptCategories.includes(slug);
  const checkedCount = categories.filter(isChecked).length;

  const toggle = async (slug: string) => {
    if (!isPlayerHost || !gameHash) {
      return;
    }
    const selected = categories.filter(isChecked);
    const next = isChecked(slug) ? selected.filter((item) => item !== slug) : [...selected, slug];
    try {
      await hub.invoke(
        HubEvents.setPromptCategories,
        gameHash,
        next.length === categories.length ? [] : next,
      );
    } catch {
      reportRealtimeError(t("error.action.changeSettings"));
    }
  };

  if (categories.length === 0) {
    return null;
  }

  return (
    <div className="mt-4">
      <label className="form-label d-block text-center">{t("settings.categories")}</label>
      <div className="d-flex justify-content-center">
        <div className="game-settings-categories">
          {categories.map((slug) => {
            const checked = isChecked(slug);
            const isLastChecked = checked && checkedCount === 1;
            return (
              <div
                className="form-check text-start"
                key={slug}
                title={isLastChecked ? t("settings.atLeastOneCategory") : undefined}
              >
                <input
                  className="form-check-input"
                  type="checkbox"
                  id={`prompt-category-${slug}`}
                  checked={checked}
                  disabled={!isPlayerHost || isLastChecked}
                  onChange={() => void toggle(slug)}
                />
                <label className="form-check-label" htmlFor={`prompt-category-${slug}`}>
                  {translateCategory(slug)}
                </label>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
};

export default CategorySetting;
