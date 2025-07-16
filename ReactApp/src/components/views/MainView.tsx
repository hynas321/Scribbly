import InputForm from "../InputForm";
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

function MainView() {
  const { username, setUsername, isValid } = useUsernameValidation(1, 18);
  const { createGame, joinGame } = useGameActions(username);
  const { scoreboardScores, isTableDisplayed } = useTopScores();

  const [isPopupVisible, setIsPopupVisible] = useState(false);
  const springs = useSpring({ from: { y: 200 }, to: { y: 0 } });

  return (
    <div className="container">
      <div className="col-lg-4 col-sm-7 col-xs-6 mx-auto text-center">
        <Popup
          title="Join the game"
          inputFormPlaceholderText="Paste the invitation URL here"
          visible={isPopupVisible}
          onSubmit={(value) => joinGame(UrlHelper.getGameHash(value))}
          onClose={() => setIsPopupVisible(false)}
        />
        <InputForm
          defaultValue={username}
          placeholderValue="Enter username"
          smallTextValue="Allowed username length 1-18"
          onChange={setUsername}
        />
        <Button text="Create the Game" type="success" active={isValid} onClick={createGame} />
        <Button text="Join the Game" active={isValid} onClick={() => setIsPopupVisible(true)} />
      </div>
      <animated.div className="col-lg-4 col-md-8 mt-5 text-center mx-auto" style={{ ...springs }}>
        {isTableDisplayed ? (
          <MainScoreboard
            title="Top 5 players"
            scoreboardScores={scoreboardScores}
            displayPoints
            displayIndex
          />
        ) : (
          <img src={tableLoading} alt="Table loading" className="img-fluid" />
        )}
      </animated.div>
    </div>
  );
}

export default MainView;