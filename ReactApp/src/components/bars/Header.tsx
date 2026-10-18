import { BsPencilFill } from "react-icons/bs";
import Account from "../Account";
import { Language, useTranslation } from "../../i18n/I18nContext";

const languages: Array<{ code: Language; label: string }> = [
  { code: "en", label: "EN" },
  { code: "pl", label: "PL" },
];

function Header() {
  const { language, setLanguage, t } = useTranslation();

  return (
    <div className="container-fluid">
      <div className="row mb-3">
        <div
          className="col-12 col-md-3 mt-3 d-flex align-items-center justify-content-center
            justify-content-md-start"
        >
          <div className="btn-group" role="group" aria-label={t("header.languageLabel")}>
            {languages.map((option) => (
              <button
                key={option.code}
                type="button"
                className={`btn btn-sm ${
                  language === option.code ? "btn-success" : "btn-outline-success"
                }`}
                onClick={() => setLanguage(option.code)}
              >
                {option.label}
              </button>
            ))}
          </div>
        </div>
        <div className="col-12 col-md-6">
          <h1 className="text-success text-center mt-3">
            <b>
              Scribbly <BsPencilFill />
            </b>
          </h1>
        </div>
        <div className="col-12 col-md-3 mt-3">
          <div className="d-flex justify-content-center justify-content-md-end">
            <Account />
          </div>
        </div>
      </div>
    </div>
  );
}

export default Header;
