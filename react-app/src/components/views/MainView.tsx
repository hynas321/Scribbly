import { useContext, useEffect, useState } from 'react';
import Button from '../Button';
import InputForm from '../InputForm';
import { useAppDispatch, useAppSelector } from '../../redux/hooks';
import config from '../../../config.json';
import Alert from '../Alert';
import { useNavigate } from 'react-router-dom';
import { Player, updatedGameHash, updatedPlayer } from '../../redux/slices/player-slice';
import PlayerList from '../PlayerList';
import Popup from '../Popup';
import HttpRequestHandler from '../../utils/HttpRequestHandler';
import UrlHelper from '../../utils/UrlHelper';
import { CreateGameRequestResponse } from '../../utils/RequestInterfaces';

function MainView() {
  const httpRequestHandler = new HttpRequestHandler();
  const urlHelper = new UrlHelper();
  const minUsernameLength: number = 1;

  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const player = useAppSelector((state) => state.player);

  const [username, setUsername] = useState("");
  const [createLobbyActiveButton, setCreateLobbyActiveButton] = useState(false);
  const [joinLobbyActiveButton, setJoinLobbyActiveButton] = useState(false);
  const [alertText, setAlertText] = useState("");
  const [alertVisible, setAlertVisible] = useState(false);
  const [alertType, setAlertType] = useState("primary");
  const [playerListVisible, setPlayerListVisible] = useState(false);
  const [popupVisible, setPopupVisible] = useState(false);
  const [playerList, setPlayerList] = useState<Player[]>([]);
  const [joiningGame, setJoiningGame] = useState(false);

  const createGame = async () => {
    await httpRequestHandler.createGame(username)
      .then(async (data: CreateGameRequestResponse) => {
        const player: Player = {
          username: username,
          score: 0,
          token: data.hostToken,
          gameHash: data.gameHash
        }

        dispatch(updatedPlayer(player));
        setJoiningGame(true);
      })
      .catch(() => {
        console.log("S")
        displayAlert("Error", "danger");
      });
  }

  const navigateToProperPage = async () => {
    await httpRequestHandler.checkIfGameIsStarted(player.gameHash)
      .then((data: boolean) => {

        if (typeof data != "boolean") {
          setPopupVisible(false);
          setJoiningGame(false);
          displayAlert("Game does not exist", "danger");
          return;
        }

        if (data === true) {
          navigate(config.gameClientEndpoint);
        }
        else {
          navigate(config.lobbyClientEndpoint);
        }
      })
      .catch(() => {
        navigate(config.mainClientEndpoint);
        displayAlert("Something went wrong, try again", "danger");
      });
  }

  const displayAlert = (message: string, type: string) => {
    setAlertText(message);
    setAlertType(type);
    setAlertVisible(true);

    setTimeout(() => {
      setAlertVisible(false);
    }, 2500);
  }

  const handleInputFormChange = (value: string) => {
    setUsername(value.trim());
  }

  const handleCreateGameButtonClick = async () => {
    if (username.length < minUsernameLength) {
      return;
    }
    
    await createGame();
  }

  const handleJoinGameButtonClick = async () => {
    if (username.length < minUsernameLength) {
      return;
    }

    setPopupVisible(true);
  }

  const handleClosePopup = () => {
    setPopupVisible(false);
  }

  const handleOnSubmitPopup = async (value: string) => {
    dispatch(updatedGameHash(urlHelper.getGameHash(value)));
    setJoiningGame(true);
  }

  useEffect(() => {
    const fetchPlayerScores = async () => {
      await httpRequestHandler.fetchPlayerScores()
      .then((data) => {
        if (Array.isArray(data)) {
          setPlayerList(data);
          setPlayerListVisible(true);
        }
        else {
          setPlayerListVisible(false);
        }
      });
    }

    fetchPlayerScores();
  }, []);

  useEffect(() => {
    if (username.length >= minUsernameLength) {
      setCreateLobbyActiveButton(true);
      setJoinLobbyActiveButton(true);
    } 
    else {
      setCreateLobbyActiveButton(false);
      setJoinLobbyActiveButton(false);
    }
  }, [username]);

  useEffect(() => {
    if (joiningGame === true) {
      navigateToProperPage();
    }
  }, [joiningGame]);

  return (
    <div className="container">
      <div className="col-lg-4 col-sm-7 col-xs-6 mx-auto text-center">
        <Popup 
          title={"Join the lobby"}
          inputFormPlaceholderText={"Paste the invitation hash here"}
          visible={popupVisible}
          onSubmit={handleOnSubmitPopup}
          onClose={handleClosePopup}
        />
        <Alert
            visible={alertVisible}
            text={alertText}
            type={alertType}
        />
        <InputForm
          placeholderValue="Enter username"
          smallTextValue={`Minimum username length ${minUsernameLength}`}
          onChange={handleInputFormChange}
        />
        <Button
          text={"Create the lobby"}
          type="success"
          active={createLobbyActiveButton}
          onClick={handleCreateGameButtonClick}
        />
        <Button
          text="Join the lobby"
          active={joinLobbyActiveButton}
          onClick={handleJoinGameButtonClick}
        />
      </div>
      <div className="col-lg-3 col-sm-6 col-xs-6 mt-5 text-center mx-auto">
        { playerListVisible &&
          <PlayerList
            title="Top 5 players"
            players={playerList}
            displayPoints={true}
            displayIndex={true}
          /> 
        }
      </div>
    </div>
  );
}

export default MainView;