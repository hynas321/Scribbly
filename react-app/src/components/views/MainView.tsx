import { useEffect, useState } from 'react';
import Button from '../Button';
import InputForm from '../InputForm';
import GameSettingsBoard from '../GameSettingsBoard';
import { useAppDispatch, useAppSelector } from '../../redux/hooks';
import EndpointHandler from "../../utils/EndpointHandler";
import config from '../../../config.json';
import Alert from '../Alert';
import { Link, useNavigate } from 'react-router-dom';
import { Player, updatedUsername } from '../../redux/slices/player-slice';
import PlayerList from '../PlayerList';

function MainView() {
  const minUsernameLength: number = 5;
  const dispatch = useAppDispatch();

  const navigate = useNavigate();
  const [username, setUsername] = useState("");
  const [createLobbyActiveButton, setCreateLobbyActiveButton] = useState(false);
  const [joinLobbyActiveButton, setJoinLobbyActiveButton] = useState(false);
  const [alertText, setAlertText] = useState("");
  const [alertVisible, setAlertVisible] = useState(false);
  const [alertType, setAlertType] = useState("primary");
  const [modalVisible, setModalVisible] = useState(false);

  const player: Player = {
    username: "Test",
    points: 100
  }
  const handleInputFormChange = (value: string) => {
    setUsername(value.trim());
  }

  const handleCreateLobbyButtonClick = () => {
    if (username.length < minUsernameLength) {
      return;
    }

    dispatch(updatedUsername(username));
    navigate(config.createGameClientEndpoint);
  }

  const handleJoinLobbyButtonClick = () => {
    if (username.length < minUsernameLength) {
      return;
    }

    dispatch(updatedUsername(username));
    navigate(config.createGameClientEndpoint);
    //TODO
  }

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
      <div className="col-lg-4 col-sm-5 col-xs-6 mx-auto text-center">
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
          text="Create the lobby"
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
      <div className="col-2 mt-4 text-center mx-auto">
        <PlayerList
          title={"Top 10 players"}
          players={[player, player, player, player, player, player, player, player]}
          displayPoints={true}
        />
      </div>
    </div>
  );
}

export default MainView;