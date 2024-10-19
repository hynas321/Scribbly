import { useEffect, useState } from "react";
import Button from "../Button";
import InputForm from "../InputForm";
import { useAppDispatch } from "../../redux/hooks";
import config from "../../../config.json";
import { useNavigate } from "react-router-dom";
import { updatedUsername } from "../../redux/slices/player-score-slice";
import HttpRequestHandler from "../../http/HttpRequestHandler";
import useLocalStorageState from "use-local-storage-state";
import tableLoading from "./../../assets/table-loading.gif";
import MainScoreboard from "../MainScoreboard";
import UrlHelper from "../../utils/UrlHelper";
import Popup from "../Popup";
import { animated, useSpring } from "@react-spring/web";
import { MainScoreboardScore } from "../../interfaces/MainScoreboardScore";
import { toast, ToastContainer } from "react-toastify";
import { ToastNotificationEnum } from "../../enums/ToastNotificationEnum";
import "react-toastify/dist/ReactToastify.css";

function MainView() {
  const httpRequestHandler = new HttpRequestHandler();
  const minUsernameLength: number = 1;
  const maxUsernameLength: number = 18;

  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const [scoreboardScores, setScoreboardScores] = useState<MainScoreboardScore[]>([]);
  const [, setGameHash] = useState<string>("");
  const [isCreateGameButtonActive, setIsCreateGameButtonActive] = useState<boolean>(false);
  const [isJoinGameButtonActive, setIsJoinGameButtonActive] = useState<boolean>(false);
  const [isTableDisplayed, setIsTableDisplayed] = useState<boolean>(false);
  const [isPopupVisible, setIsPopupVisible] = useState<boolean>(false);

  const [, setToken] = useLocalStorageState("token", { defaultValue: "" });
  const [username, setUsername] = useLocalStorageState("username", { defaultValue: "" });

  const handleInputFormChange = (value: string) => {
    setUsername(value);
  };

  const handleCreateGameButtonClick = async () => {
    if (username.length < minUsernameLength || username.length > maxUsernameLength) {
      return;
    }

    try {
      const data = await httpRequestHandler.createGame(username);

      if (!("hostToken" in data) || data.gameHash == undefined) {
        toast.error("Could not create the game, try again", {
          containerId: ToastNotificationEnum.Main,
        });
        return;
      }

      setToken(data.hostToken);
      dispatch(updatedUsername(username));
      navigate(`${config.gameClientEndpoint}/${data.gameHash}`, {
        state: { fromViewNavigation: true },
      });
    } catch (error) {
      toast.error("Unexpected error", { containerId: ToastNotificationEnum.Main });
    }
  };

  const handleJoinLobbyButtonClick = () => {
    if (username.length < minUsernameLength) {
      return;
    }

    setIsPopupVisible(true);
  };

  const handleClosePopup = () => {
    setIsPopupVisible(false);
  };

  const handleOnSubmitPopup = async (value: string) => {
    const gameHash = UrlHelper.getGameHash(value);

    const checkIfGameExists = async () => {
      try {
        const data = await httpRequestHandler.checkIfGameExists(gameHash);

        if (typeof data != "boolean") {
          setIsPopupVisible(false);
          toast.error("Unexpected error, try again", { containerId: ToastNotificationEnum.Main });
          return;
        }

        if (data === true) {
          dispatch(updatedUsername(username));
          navigate(`${config.gameClientEndpoint}/${gameHash}`, {
            state: { fromViewNavigation: true },
          });
        } else {
          setIsPopupVisible(false);
          toast.error("Game does not exist", { containerId: ToastNotificationEnum.Main });
        }
      } catch (error) {
        setIsPopupVisible(false);
        toast.error("Unexpected error, try again", { containerId: ToastNotificationEnum.Main });
      }
    };

    await checkIfGameExists();
  };

  useEffect(() => {
    const fetchPlayerScores = async () => {
      setGameHash(UrlHelper.getGameHash(window.location.href));

      try {
        const data = await httpRequestHandler.fetchTopAccountScores();

        if (!Array.isArray(data)) {
          setIsTableDisplayed(false);
          return;
        }

        setScoreboardScores(data);
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
      setIsCreateGameButtonActive(false);
    } else {
      setIsJoinGameButtonActive(true);
      setIsCreateGameButtonActive(true);
    }
  }, [username]);

  const springs = useSpring({
    from: { y: 200 },
    to: { y: 0 },
  });

  return (
    <>
      <ToastContainer
        containerId={ToastNotificationEnum.Main}
        position="top-left"
        autoClose={3000}
        closeOnClick
        draggable
        pauseOnHover={false}
        theme="light"
        style={{ opacity: 0.9 }}
      />
      <div className="container">
        <div className="col-lg-4 col-sm-7 col-xs-6 mx-auto text-center">
          <Popup
            title={"Join the game"}
            inputFormPlaceholderText={"Paste the invitation URL here"}
            visible={isPopupVisible}
            onSubmit={handleOnSubmitPopup}
            onClose={handleClosePopup}
          />
          <InputForm
            defaultValue={username}
            placeholderValue="Enter username"
            smallTextValue={`Allowed username length ${minUsernameLength}-${maxUsernameLength}`}
            onChange={handleInputFormChange}
          />
          <Button
            text={"Create the game"}
            type="success"
            active={isCreateGameButtonActive}
            onClick={handleCreateGameButtonClick}
          />
          <Button
            text="Join the game"
            active={isJoinGameButtonActive}
            onClick={handleJoinLobbyButtonClick}
          />
        </div>
        <animated.div className="col-lg-4 col-md-8 mt-5 text-center mx-auto" style={{ ...springs }}>
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

export default MainView;
