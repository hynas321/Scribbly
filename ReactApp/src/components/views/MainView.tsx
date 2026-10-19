import TextInput from "../forms/TextInput";
import Button from "../Button";
import Popup from "../Popup";
import MainScoreboard from "../MainScoreboard";
import tableLoading from "./../../assets/table-loading.gif";
import { useSpring, animated } from "@react-spring/web";

import UrlHelper from "../../utils/UrlHelper";
import { useState } from "react";
import { useUsernameValidation } from "../../hooks/useUsernameValidation";
import { useGameActions } from "../../hooks/useGameActions";
import { useTopScores } from "../../hooks/useTopScores";
import { useTranslation } from "../../i18n/I18nContext";

function MainView() {
  const { t } = useTranslation();
  const { username, setUsername, isValid } = useUsernameValidation(1, 18);
  const { createGame, joinGame } = useGameActions(username);
  const { scoreboardScores, isTableDisplayed } = useTopScores();

  const [isPopupVisible, setIsPopupVisible] = useState(false);
  const springs = useSpring({ from: { y: 200 }, to: { y: 0 } });

  return (
    <div className="container">
      <div className="col-lg-4 col-sm-7 col-xs-6 mx-auto text-center">
        <Popup
          title={t("mainView.popupTitle")}
          inputFormPlaceholderText={t("mainView.popupPlaceholder")}
          visible={isPopupVisible}
          onSubmit={(value) => joinGame(UrlHelper.getGameHash(value))}
          onClose={() => setIsPopupVisible(false)}
        />
        <TextInput
          value={username}
          placeholderValue={t("common.usernamePlaceholder")}
          smallTextValue={t("common.usernameLengthHint", { min: 1, max: 18 })}
          onChange={setUsername}
        />
        <Button
          text={t("mainView.createGame")}
          type="success"
          active={isValid}
          onClick={createGame}
        />
        <Button
          text={t("mainView.joinGame")}
          active={isValid}
          onClick={() => setIsPopupVisible(true)}
        />
      </div>
      <animated.div className="col-lg-4 col-md-8 mt-5 text-center mx-auto" style={{ ...springs }}>
        {isTableDisplayed ? (
          <MainScoreboard
            title={t("scoreboard.topPlayers")}
            scoreboardScores={scoreboardScores}
            displayPoints
            displayIndex
          />
        ) : (
          <img src={tableLoading} alt={t("scoreboard.loadingAlt")} className="img-fluid" />
        )}
      </animated.div>
    </div>
  );
}

export default MainView;
