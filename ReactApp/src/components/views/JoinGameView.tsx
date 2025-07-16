import InputForm from "../InputForm";
import Button from "../Button";
import MainScoreboard from "../MainScoreboard";
import tableLoading from "./../../assets/table-loading.gif";
import { animated, useSpring } from "@react-spring/web";
import config from "../../../config.json";
import { useJoinGameState } from "../../hooks/useJoinGameState";
import { useNavigate } from "react-router-dom";
import { useTopScores } from "../../hooks/useTopScores";

const JoinGameView = () => {
  const navigate = useNavigate();
  const { scoreboardScores, isTableDisplayed } = useTopScores();
  const {
    username,
    setUsername,
    isJoinGameButtonActive,
    handleJoinGame,
    minUsernameLength,
    maxUsernameLength
  } = useJoinGameState();

  const animationSpring = useSpring({
    from: { y: 200 },
    to: { y: 0 },
  });

  return (
    <div className="container">
      <div className="col-lg-4 col-sm-7 col-xs-6 mx-auto text-center">
        <InputForm
          defaultValue={username}
          placeholderValue="Enter username"
          smallTextValue={`Allowed username length ${minUsernameLength}-${maxUsernameLength}`}
          onChange={setUsername}
        />
        <Button
          text={"Join the Game"}
          active={isJoinGameButtonActive}
          onClick={handleJoinGame}
        />
        <Button
          text={"To The Main Page"}
          active={true}
          type="secondary"
          onClick={() => navigate(config.mainClientEndpoint)}
        />
      </div>
      <animated.div
        className="col-lg-4 col-md-8 mt-5 text-center mx-auto"
        style={{ ...animationSpring }}
      >
        {isTableDisplayed ? (
          <MainScoreboard
            title="Top 5 players"
            scoreboardScores={scoreboardScores}
            displayPoints={true}
            displayIndex={true}
          />
        ) : (
          <img src={tableLoading} alt="Table loading" className="img-fluid" />
        )}
      </animated.div>
    </div>
  );
}

export default JoinGameView;