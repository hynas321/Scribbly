import { useEffect, useState } from 'react';
import Button from '../Button';
import InputForm from '../InputForm';
import { useAppDispatch } from '../../redux/hooks';
import config from '../../../config.json';
import Alert from '../Alert';
import { useNavigate } from 'react-router-dom';
import { Player, updatedGameHash, updatedToken, updatedUsername } from '../../redux/slices/player-slice';
import PlayerList from '../PlayerList';
import Popup from '../Popup';
import HttpRequestHandler from '../../utils/HttpRequestHandler';

function MainView() {
  const httpRequestHandler = new HttpRequestHandler();
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

    dispatch(updatedUsername(username));
    dispatch(updatedGameHash("TestGameHash"));
    dispatch(updatedToken("HostToken"));
    navigate(config.createGameClientEndpoint);
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
    dispatch(updatedGameHash("TestGameHash"));
    dispatch(updatedToken("TestToken"));
    navigate(config.createGameClientEndpoint);
  }

  useEffect(() => {
    httpRequestHandler.fetchPlayerScores()
    .then((data) => {
      if (Array.isArray(data)) {
        setPlayerList(data);
      }
    });
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