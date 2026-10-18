import sadFace from "../../assets/sad-face.png";
import Button from "../Button";
import config from "../../../config.json";
import { Link } from "react-router-dom";
import { useTranslation } from "../../i18n/I18nContext";

function PageNotFoundView() {
  const { t } = useTranslation();
  return (
    <div className="container text-center mt-3">
      <h3 className="mt-3">{t("pageNotFound.title")}</h3>
      <div>
        <Link to={config.mainClientEndpoint}>
          <Button text={t("pageNotFound.goToMainPage")} active={true} type="danger" />
        </Link>
      </div>
      <img src={sadFace} alt={t("pageNotFound.sadFaceAlt")} className="img-fluid" />
    </div>
  );
}

export default PageNotFoundView;
