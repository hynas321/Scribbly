import { useContext, useEffect, useState } from 'react';
import Button from '../Button';
import InputForm from '../InputForm';
import { useAppDispatch } from '../../redux/hooks';
import config from '../../../config.json';
import Alert from '../Alert';
import { useNavigate } from 'react-router-dom';
import { Player, updatedGameHash, updatedPlayer, updatedUsername } from '../../redux/slices/player-slice';
import PlayerList from '../PlayerList';
import Popup from '../Popup';
import HttpRequestHandler from '../../utils/HttpRequestHandler';
import UrlHelper from '../../utils/UrlHelper';
import { ConnectionHubContext } from '../../context/ConnectionHubContext';

function MainView() {
  const hub = useContext(ConnectionHubContext);
  const httpRequestHandler = new HttpRequestHandler();
  const urlHelper = new UrlHelper();
  const minUsernameLength: number = 5;
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const [username, setUsername] = useState("");
  const [createLobbyActiveButton, setCreateLobbyActiveButton] = useState(false);
  const [joinLobbyActiveButton, setJoinLobbyActiveButton] = useState(false);
  const [alertText, setAlertText] = useState("");
  const [alertVisible, setAlertVisible] = useState(false);
  const [alertType, setAlertType] = useState("primary");
  const [popupVisible, setPopupVisible] = useState(false);
  const [playerList, setPlayerList] = useState<Player[]>([]);

  const handleInputFormChange = (value: string) => {
    setUsername(value.trim());
  }

  const handleCreateLobbyButtonClick = () => {
    if (username.length < minUsernameLength) {
      return;
    }

    const createGame = async () => {
      await httpRequestHandler.createGame(username)
        .then((data: CreateLobbyRequestResponse) => {
          const player: Player = {
            username: username,
            token: data.hostToken,
            gameHash: data.gameHash,
            score: 0
          }

          console.log(data);

          dispatch(updatedPlayer(player));
          navigate(`${config.gameClientEndpoint}`);
        });
    }

    createGame();
  }

  const handleJoinLobbyButtonClick = () => {
    if (username.length < minUsernameLength) {
      return;
    }

    setPopupVisible(true);
  }

  const handleClosePopup = () => {
    setPopupVisible(false);
  }

  const handleOnSubmitPopup = (value: string) => {
    dispatch(updatedUsername(username));
    dispatch(updatedGameHash(urlHelper.getGameHash(value)));

    navigate(config.gameClientEndpoint);
  }

  useEffect(() => {
    const fetchPlayerScores = async () => {
      await httpRequestHandler.fetchPlayerScores()
      .then((data) => {
        if (Array.isArray(data)) {
          setPlayerList(data);
        }
      });
    }

    hub.start();
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
        <PlayerList
          title="Top 5 players"
          players={playerList}
          displayPoints={true}
          displayIndex={true}
        />
      </div>
    </div>
  );
}

export default MainView;