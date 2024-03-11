import useLocalStorageState from "use-local-storage-state";
import InputForm from "../InputForm";
import Button from "../Button";
import { useEffect, useState } from "react";
import HttpRequestHandler from "../../http/HttpRequestHandler";
import { useNavigate } from "react-router-dom";
import { useDispatch } from "react-redux";
import { updatedAlert, updatedVisible } from "../../redux/slices/alert-slice";
import { updatedUsername } from "../../redux/slices/player-score-slice";
import UrlHelper from "../../utils/UrlHelper";
import config from '../../../config.json';
import MainScoreboard from "../MainScoreboard";
import tableLoading from './../../assets/table-loading.gif'
import Alert from "../Alert";
import { animated, useSpring } from "@react-spring/web";

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

  const [username, setUsername] = useLocalStorageState("username", { defaultValue: ""});

  const animationSpring = useSpring({
    from: { y: 200 },
    to: { y: 0 },
  });

  const handleInputFormChange = (value: string) => {
    setUsername(value);
  }

  const handleJoinGameButtonClick = async () => {
    if (username.length < minUsernameLength) {
      return;
    }

    const checkIfGameExists = async () => {
      try {
        const data = await httpRequestHandler.checkIfGameExists(gameHash);
    
        if (typeof data != "boolean") {
          displayAlert("Unexpected error", "danger");
          navigate(`${config.mainClientEndpoint}`);
          return;
        }
    
        if (data === true) {
          dispatch(updatedUsername(username));
          navigate(`${config.gameClientEndpoint}/${gameHash}`, { state: { fromViewNavigation: true } });
        }
        else {
          displayAlert("Game does not exist", "danger");
          navigate(`${config.mainClientEndpoint}`);
        }
      
      }
      catch (error) {
        displayAlert("Unexpected error", "danger");
        navigate(`${config.mainClientEndpoint}`);
      }
    };

    checkIfGameExists();
  }

  useEffect(() => {
    setGameHash(UrlHelper.getGameHash(window.location.href));

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

      }
      catch (error) {
        console.error(error);
      }
    };

    fetchPlayerScores();
  }, []);

  useEffect(() => {
    if (username.length < minUsernameLength ||
        username.length > maxUsernameLength) {
      setIsJoinGameButtonActive(false);
    } 
    else {
      setIsJoinGameButtonActive(true);
    }
  }, [username]);

  const displayAlert = (message: string, type: string) => {
    dispatch(updatedAlert({
      text: message,
      visible: true,
      type: type
    }));

    setTimeout(() => {
      dispatch(updatedVisible(false));
    }, 3000);
  }
  
  return (
    <div className="container">
      <div className="col-lg-4 col-sm-7 col-xs-6 mx-auto text-center">
        <Alert />
        <InputForm
          defaultValue={username}
          placeholderValue="Enter username"
          smallTextValue={`Allowed username length ${minUsernameLength}-${maxUsernameLength}`}
          onChange={handleInputFormChange}
        />
        <Button
          text={"Join the game"}
          active={isJoinGameButtonActive}
          onClick={handleJoinGameButtonClick}
        />
      </div>
      <animated.div
        className="col-lg-3 col-sm-6 col-xs-6 mt-5 text-center mx-auto"
        style={{...animationSpring}}>
        { isTableDisplayed ?
          <MainScoreboard
            title="Top 5 players"
            scoreboardScores={scoreboardScores}
            displayPoints={true}
            displayIndex={true}
          /> 
          :
          <img src={tableLoading} alt="Table loading" className="img-fluid" />
        }
      </animated.div>
    </div>
  )
}

export default JoinGameView;