import InputForm from "../InputForm";
import Button from "../Button";
import { useEffect, useState } from "react";
import HttpRequestHandler from "../../http/HttpRequestHandler";
import { useNavigate } from "react-router-dom";
import { useDispatch } from "react-redux";
import { updatedUsername } from "../../redux/slices/player-score-slice";
import UrlHelper from "../../utils/UrlHelper";
import config from "../../../config.json";
import MainScoreboard from "../MainScoreboard";
import tableLoading from "./../../assets/table-loading.gif";
import { animated, useSpring } from "@react-spring/web";
import { MainScoreboardScore } from "../../interfaces/MainScoreboardScore";
import { toast } from "react-toastify";
import { ToastNotificationEnum } from "../../enums/ToastNotificationEnum";
import { useSessionStorageUsername } from "../../hooks/useSessionStorageUsername";

function JoinGameView() {
  const httpRequestHandler = new HttpRequestHandler();
  const minUsernameLength: number = 1;
  const maxUsernameLength: number = 18;

  const navigate = useNavigate();
  const dispatch = useDispatch();

  const [gameHash, setGameHash] = useState<string>("");
  const [isJoinGameButtonActive, setIsJoinGameButtonActive] = useState<boolean>(false);
  const [isTableDisplayed, setIsTableDisplayed] = useState<boolean>(false);
  const [scoreboardScores, setScoreboardScores] = useState<MainScoreboardScore[]>([]);

  const [username, setUsername] = useSessionStorageUsername();

  const animationSpring = useSpring({
    from: { y: 200 },
    to: { y: 0 },
  });

  const handleInputFormChange = (value: string) => {
    setUsername(value);
  };

  const handleJoinGameButtonClick = async () => {
    if (username.length < minUsernameLength) {
      return;
    }

    const checkIfGameExists = async () => {
      try {
        const gameExists = await httpRequestHandler.checkIfGameExists(gameHash);

        if (!gameExists) {
          toast.error("Game does not exist", { containerId: ToastNotificationEnum.Main });
          navigate(`${config.mainClientEndpoint}`);
        }

        dispatch(updatedUsername(username));
        navigate(`${config.gameClientEndpoint}/${gameHash}`, {
          state: { fromViewNavigation: true },
        });
      } catch (error) {
        toast.error("Unexpected error", { containerId: ToastNotificationEnum.Main });
        navigate(`${config.mainClientEndpoint}`);
      }
    };

    checkIfGameExists();
  };

  useEffect(() => {
    setGameHash(UrlHelper.getGameHash(window.location.href));

    const fetchPlayerScores = async () => {
      setGameHash(UrlHelper.getGameHash(window.location.href));

      try {
        const topScores = await httpRequestHandler.fetchTopAccountScores();

        if (!Array.isArray(topScores)) {
          setIsTableDisplayed(false);
          return;
        }

        setScoreboardScores(topScores);
        setTimeout(() => {
          setIsTableDisplayed(true);
        }, 1000);
      } catch (error) {
        console.error(error);
      }
    };

    fetchPlayerScores();
  }, []);

  useEffect(() => {
    if (username.length < minUsernameLength || username.length > maxUsernameLength) {
      setIsJoinGameButtonActive(false);
    } else {
      setIsJoinGameButtonActive(true);
    }
  }, [username]);

  return (
    <>
      <div className="container">
        <div className="col-lg-4 col-sm-7 col-xs-6 mx-auto text-center">
          <InputForm
            defaultValue={username}
            placeholderValue="Enter username"
            smallTextValue={`Allowed username length ${minUsernameLength}-${maxUsernameLength}`}
            onChange={handleInputFormChange}
          />
          <Button
            text={"Join the Game"}
            active={isJoinGameButtonActive}
            onClick={handleJoinGameButtonClick}
          />
          <Button
            text={"To The Main Page"}
            active={isJoinGameButtonActive}
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
    </>
  );
}

export default JoinGameView;
