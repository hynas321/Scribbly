import { useEffect, useState } from 'react';
import Button from '../Button';
import InputForm from '../InputForm';
import { useAppDispatch } from '../../redux/hooks';
import config from '../../../config.json';
import Alert from '../Alert';
import { useNavigate } from 'react-router-dom';
import { updatedUsername, } from '../../redux/slices/player-score-slice';
import HttpRequestHandler from '../../http/HttpRequestHandler';
import { updatedAlert, updatedVisible } from '../../redux/slices/alert-slice';
import useLocalStorageState from 'use-local-storage-state';
import tableLoading from './../../assets/table-loading.gif';
import MainScoreboard from '../MainScoreboard';
import UrlHelper from '../../utils/UrlHelper';
import Popup from '../Popup';
import { animated, useSpring } from '@react-spring/web';
import { MainScoreboardScore } from '../../types/MainScoreboardScore';

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
  const [username, setUsername] = useLocalStorageState("username", { defaultValue: ""});

  const handleInputFormChange = (value: string) => {
    setUsername(value);
  }

  const handleCreateGameButtonClick = async () => {
    if (username.length < minUsernameLength ||
        username.length > maxUsernameLength) {
      return;
    }
  
    try {
      const data = await httpRequestHandler.createGame(username);

      if (!("gameHash" || "hostToken" in data) || data.gameHash == undefined) {
        displayAlert("Could not create the game, try again.", "primary");
        return;
      }

      setToken(data.hostToken);
      dispatch(updatedUsername(username));
      navigate(`${config.gameClientEndpoint}/${data.gameHash}`, { state: { fromViewNavigation: true } });

    }
    catch (error) {
      displayAlert("Error", "danger");
    }
  }

  const handleJoinLobbyButtonClick = () => {
    if (username.length < minUsernameLength) {
      return;
    }

    setIsPopupVisible(true);
  }

  const handleClosePopup = () => {
    setIsPopupVisible(false);
  }

  const handleOnSubmitPopup = async (value: string) => {
    const gameHash = UrlHelper.getGameHash(value);

    const checkIfGameExists = async () => {
      try {
        const data = await httpRequestHandler.checkIfGameExists(gameHash);
    
        if (typeof data != "boolean") {
          setIsPopupVisible(false);
          displayAlert("Unexpected error, try again", "danger");
          return;
        }
    
        if (data === true) {
          dispatch(updatedUsername(username));
          navigate(`${config.gameClientEndpoint}/${gameHash}`, { state: { fromViewNavigation: true } });
        }
        else {
          setIsPopupVisible(false);
          displayAlert("Game does not exist", "danger");
        }
      
      }
      catch (error) {
        setIsPopupVisible(false);
        displayAlert("Unexpected error, try again", "danger");
      }
    };

    await checkIfGameExists();
  }

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
      setIsCreateGameButtonActive(false);
    } 
    else {
      setIsJoinGameButtonActive(true);
      setIsCreateGameButtonActive(true);
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

  const springs = useSpring({
    from: { y: 200 },
    to: { y: 0 },
  })

  return (
    <div className="container">
      <div className="col-lg-4 col-sm-7 col-xs-6 mx-auto text-center">
        <Popup 
          title={"Join the game"}
          inputFormPlaceholderText={"Paste the invitation URL here"}
          visible={isPopupVisible}
          onSubmit={handleOnSubmitPopup}
          onClose={handleClosePopup}
        />
        <Alert />
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
      <animated.div className="col-lg-4 col-md-8 mt-5 text-center mx-auto" style={{...springs}}>
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
  );
}

export default MainView;