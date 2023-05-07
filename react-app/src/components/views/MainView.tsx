import { useContext, useEffect, useRef, useState } from 'react';
import Button from '../Button';
import InputForm from '../InputForm';
import { useAppDispatch } from '../../redux/hooks';
import config from '../../../config.json';
import Alert from '../Alert';
import { useNavigate } from 'react-router-dom';
import { PlayerScore, updatedUsername, } from '../../redux/slices/player-score-slice';
import PlayerList from '../PlayerList';
import HttpRequestHandler from '../../http/HttpRequestHandler';
import { updatedAlert, updatedVisible } from '../../redux/slices/alert-slice';
import useLocalStorageState from 'use-local-storage-state';
import { ConnectionHubContext } from '../../context/ConnectionHubContext';
import * as signalR from '@microsoft/signalr'
import tableLoading from './../../assets/table-loading.gif'

function MainView() {
  const hub = useContext(ConnectionHubContext);
  const httpRequestHandler = new HttpRequestHandler();
  const minUsernameLength: number = 1;

  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const [createGameActiveButton, setCreateGameActiveButton] = useState(false);
  const [joinGameActiveButton, setJoinGameActiveButton] = useState(false);
  const [isTableDisplayed, setIsTableDisplayed] = useState(false);
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
  
    try {
      const data = await httpRequestHandler.createGame(username);

      if (!("hostToken" in data)) {
        displayAlert("The game is already created. Join the game.", "primary");
        return;
      }

      setToken(data.hostToken);
      dispatch(updatedUsername(username));
      navigate(config.gameClientEndpoint);

    }
    catch (error) {
      displayAlert("Error", "danger");
    }
  }

  const handleJoinGameButtonClick = async () => {
    if (username.length < minUsernameLength) {
      return;
    }

    const checkIfGameExists = async () => {
      try {
        const data = await httpRequestHandler.checkIfGameExists();
    
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
      
      }
      catch (error) {
        displayAlert("Unexpected error, try again", "danger");
      }
    };

    await checkIfGameExists();
  }

  useEffect(() => {
    const fetchPlayerScores = async () => {
      try {
        const data = await httpRequestHandler.fetchPlayerScores();
        
        if (!Array.isArray(data)) {
          setIsTableDisplayed(false);
          return;
        }
      
        setPlayerScores(data);
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
    if (username.length < minUsernameLength) {
      setJoinGameActiveButton(false);
      setCreateGameActiveButton(false);
    } 
    else {
      setJoinGameActiveButton(true);
      setCreateGameActiveButton(true);
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
        { isTableDisplayed ?
          <PlayerList
            title="Top 5 players"
            playerScores={playerScores}
            displayPoints={true}
            displayIndex={true}
          /> 
          :
          <img src={tableLoading} alt="Table loading" className="img-fluid" />
        }
      </div>
    </div>
  );
}

export default MainView;