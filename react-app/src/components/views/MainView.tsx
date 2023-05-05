import { useEffect, useRef, useState } from 'react';
import Button from '../Button';
import InputForm from '../InputForm';
import { useAppDispatch } from '../../redux/hooks';
import config from '../../../config.json';
import Alert from '../Alert';
import { useNavigate } from 'react-router-dom';
import { PlayerScore, updatedUsername, } from '../../redux/slices/player-score-slice';
import PlayerList from '../PlayerList';
import HttpRequestHandler from '../../http/HttpRequestHandler';
import { CreateGameResponse } from '../../http/HttpInterfaces';
import { updatedAlert, updatedVisible } from '../../redux/slices/alert-slice';
import useLocalStorageState from 'use-local-storage-state';


function MainView() {
  const httpRequestHandler = new HttpRequestHandler();
  const minUsernameLength: number = 1;

  const isInitialEffectRender = useRef(true);
  const isUsernameEffectRendered = useRef(false);

  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const [createGameActiveButton, setCreateGameActiveButton] = useState(false);
  const [joinGameActiveButton, setJoinGameActiveButton] = useState(false);
  const [playerListVisible, setPlayerListVisible] = useState(false);
  const [playerScores, setPlayerScores] = useState<PlayerScore[]>([]);

  const [token, setToken] = useLocalStorageState("token", { defaultValue: "" });
  const [username, setUsername] = useLocalStorageState("username", { defaultValue: ""});

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
          
          if (!("hostToken" in data)) {
            displayAlert("The game is already created. Join the game.", "primary");
            return;
          }

          setToken(data.hostToken);
          dispatch(updatedUsername(username));
          navigate(config.gameClientEndpoint);
          console.log(data.hostToken);
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
            dispatch(updatedUsername(username));
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

    // const checkIfPlayerExists = async () => {
    //   await httpRequestHandler.checkIfPlayerExists(token)
    //     .then((data: boolean) => {

    //       if (data === true) {
    //         console.log(token);
    //         navigate(config.gameClientEndpoint);
    //       }
    //     });
    // }

    const fetchPlayerScores = async () => {
      await httpRequestHandler.fetchPlayerScores()
        .then((data) => {
          if (!Array.isArray(data)) {
            setPlayerListVisible(false);
            return;
          }

          setPlayerScores(data);
          setPlayerListVisible(true);
      });
    }

    //checkIfPlayerExists();
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
      setJoinGameActiveButton(false);
      setCreateGameActiveButton(false);

      console.log("Buttons inactive");
    } 
    else {
      setJoinGameActiveButton(true);
      setCreateGameActiveButton(true);

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
          defaultValue={username}
          placeholderValue="Enter username"
          smallTextValue={`Minimum username length ${minUsernameLength}`}
          onChange={handleInputFormChange}
        />
        <Button
          text={"Create the game"}
          type="success"
          active={createGameActiveButton}
          onClick={handleCreateGameButtonClick}
        />
        <Button
          text="Join the game"
          active={joinGameActiveButton}
          onClick={handleJoinGameButtonClick}
        />
      </div>
      <div className="col-lg-3 col-sm-6 col-xs-6 mt-5 text-center mx-auto">
        { playerListVisible &&
          <PlayerList
            title="Top 5 players"
            playerScores={playerScores}
            displayPoints={true}
            displayIndex={true}
          /> 
        }
      </div>
    </div>
  );
}

export default MainView;