import { useEffect, useRef, useState } from 'react';
import Button from '../Button';
import InputForm from '../InputForm';
import { useAppDispatch } from '../../redux/hooks';
import config from '../../../config.json';
import Alert from '../Alert';
import { useNavigate } from 'react-router-dom';
import { Player, } from '../../redux/slices/player-slice';
import PlayerList from '../PlayerList';
import HttpRequestHandler from '../../http/HttpRequestHandler';
import { CreateGameResponse, JoinGameResponse } from '../../http/HttpInterfaces';
import { updatedAlert, updatedVisible } from '../../redux/slices/alert-slice';
import VerificationHelper from '../../utils/VerificationHelper';
import useLocalStorageState from 'use-local-storage-state';

function MainView() {
  const httpRequestHandler = new HttpRequestHandler();
  const verificationHelper = new VerificationHelper();
  const minUsernameLength: number = 1;

  const isInitialEffectRender = useRef(true);
  const isUsernameEffectRendered = useRef(false);

  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const [username, setUsername] = useState("");
  const [createLobbyActiveButton, setCreateLobbyActiveButton] = useState(false);
  const [joinLobbyActiveButton, setJoinLobbyActiveButton] = useState(false);
  const [playerListVisible, setPlayerListVisible] = useState(false);
  const [playerList, setPlayerList] = useState<Player[]>([]);
  const [token, setToken] = useLocalStorageState("token");

  const handleInputFormChange = (value: string) => {
    setUsername(value.trim());
  }

  const handleCreateGameButtonClick = async () => {
    if (username.length < minUsernameLength) {
      return;
    }

    const createGame = async () => {
      await httpRequestHandler.createGame(username)
        .then(async (data: CreateGameResponse) => {
          
          setToken(data.hostToken);
          navigate(config.gameClientEndpoint);
        })
        .catch(() => {
          displayAlert("Error", "danger");
        });
    }
    
    await createGame();
  }

  const handleJoinGameButtonClick = async () => {
    if (username.length < minUsernameLength) {
      return;
    }

    const checkIfGameExists = async () => {
      await httpRequestHandler.checkIfGameExists()
        .then((data: boolean) => {
  
          if (typeof data != "boolean") {
            displayAlert("Unexpected error, try again", "danger");
            return;
          }
  
          if (data === true) {
            navigate(config.gameClientEndpoint);
          }
          else {
            displayAlert("Game does not exist", "danger");
          }
        })
        .catch(() => {
          displayAlert("Unexpected error, try again", "danger");
        });
    }

    await checkIfGameExists();
  }

  useEffect(() => {
    if (!isInitialEffectRender.current) {
      return;
    }

    const fetchPlayerScores = async () => {
      await httpRequestHandler.fetchPlayerScores()
        .then((data) => {
          if (!Array.isArray(data)) {
            setPlayerListVisible(false);
            return;
          }

          setPlayerList(data);
          setPlayerListVisible(true);
      });
    }

    fetchPlayerScores();
    console.log("Player scores fetched");
    isInitialEffectRender.current = false;
  }, []);

  
  useEffect(() => {
    if (!isUsernameEffectRendered.current) {
      isUsernameEffectRendered.current = true;
      return;
    }
  
    if (username.length < minUsernameLength) {
      setCreateLobbyActiveButton(false);
      setJoinLobbyActiveButton(false);

      console.log("Buttons inactive");
    } 
    else {
      setCreateLobbyActiveButton(true);
      setJoinLobbyActiveButton(true);

      console.log("Buttons active");
    }
  
    isUsernameEffectRendered.current = false;
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
            displayPoints={true}
            displayIndex={true}
          /> 
        }
      </div>
    </div>
  );
}

export default MainView;