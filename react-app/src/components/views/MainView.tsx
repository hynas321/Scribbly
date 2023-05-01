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
import { CreateGameRequestResponse, JoinGameRequestResponse } from '../../utils/RequestInterfaces';
import { updatedPlayerList } from '../../redux/slices/game-state-slice';
import { ConnectionHubContext } from '../../context/ConnectionHubContext';

function MainView() {
  const hub = useContext(ConnectionHubContext);
  const httpRequestHandler = new HttpRequestHandler();
  const urlHelper = new UrlHelper();
  const minUsernameLength: number = 5;

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

  const createGame = async () => {
    await httpRequestHandler.createGame(username)
      .then((data: CreateGameRequestResponse) => {
        const player: Player = {
          username: username,
          score: 0,
          token: data.hostToken,
          gameHash: data.gameHash
        }

        dispatch(updatedPlayer(player));
      })
      .catch(() => {
        displayAlert("Error", "danger");
      });
  }

  const joinGame = async() => {
    await httpRequestHandler.joinGame(player.gameHash, username)
      .then((data: JoinGameRequestResponse) => {
        dispatch(updatedPlayer(data.player));
        dispatch(updatedPlayerList(data.playerList));
        console.log(data)
        if (data.gameIsStarted) {
          navigate(`${config.gameClientEndpoint}`);
        }
        else {
          navigate(`${config.lobbyClientEndpoint}`);
        }
        
      })
      .catch(() => {
        displayAlert("Error", "danger");
      })
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

  const handleCreateLobbyButtonClick = async () => {
    if (username.length < minUsernameLength) {
      return;
    }
    
    await createGame();
    await joinGame();
  }

  const handleJoinLobbyButtonClick = async () => {
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

    await joinGame();
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

    hub.start();
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

  return (
    <div className="container">
      <div className="col-lg-4 col-sm-7 col-xs-6 mx-auto text-center">
        <Popup 
          title={"Join the lobby"}
          inputFormPlaceholderText={"Paste the invitation URL here"}
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
          onClick={handleCreateLobbyButtonClick}
        />
        <Button
          text="Join the lobby"
          active={joinLobbyActiveButton}
          onClick={handleJoinLobbyButtonClick}
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