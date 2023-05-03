import { useContext, useEffect, useRef, useState } from 'react';
import Button from '../Button';
import InputForm from '../InputForm';
import { useAppDispatch, useAppSelector } from '../../redux/hooks';
import config from '../../../config.json';
import Alert from '../Alert';
import { useNavigate } from 'react-router-dom';
import { Player, updatedGameHash, updatedPlayer, updatedUsername } from '../../redux/slices/player-slice';
import PlayerList from '../PlayerList';
import Popup from '../Popup';
import UrlHelper from '../../utils/UrlHelper';
import HttpRequestHandler from '../../http/HttpRequestHandler';
import { CreateGameResponse, JoinGameResponse } from '../../http/HttpInterfaces';
import { updatedAlert, updatedVisible } from '../../redux/slices/alert-slice';
import { updatedPlayerList } from '../../redux/slices/game-state-slice';

function MainView() {
  const httpRequestHandler = new HttpRequestHandler();
  const urlHelper = new UrlHelper();
  const minUsernameLength: number = 1;

  const isInitialEffectRender = useRef(true);
  const isUsernameEffectRendered = useRef(false);

  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const player = useAppSelector((state) => state.player);

  const [username, setUsername] = useState("");
  const [createLobbyActiveButton, setCreateLobbyActiveButton] = useState(false);
  const [joinLobbyActiveButton, setJoinLobbyActiveButton] = useState(false);
  const [playerListVisible, setPlayerListVisible] = useState(false);
  const [popupVisible, setPopupVisible] = useState(false);
  const [playerList, setPlayerList] = useState<Player[]>([]);
  const [joiningGame, setJoiningGame] = useState(false);
  const [navigatingToProperPage, setNavigatingToProperPage] = useState(false);

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
          const player: Player = {
            username: username,
            score: 0,
            token: data.hostToken,
            gameHash: data.gameHash
          }
          
          console.log(`Game created, hash: ${data.gameHash}`);
          dispatch(updatedPlayer(player));
          setJoiningGame(true);
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

    setPopupVisible(true);
  }

  const handleClosePopup = () => {
    setPopupVisible(false);
  }

  const handleOnSubmitPopup = async (value: string) => {
    dispatch(updatedUsername(username));
    dispatch(updatedGameHash(urlHelper.getGameHash(value)));
    setJoiningGame(true);
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

  useEffect(() => {
    if (!joiningGame) {
      return;
    }

    const joinGame = async () => {
      await httpRequestHandler.joinGame(player.token, player.gameHash, player.username)
        .then((data: JoinGameResponse) => {
          dispatch(updatedPlayerList(data.playerScores));
          dispatch(updatedGameHash(data.gameHash));
        })
        .catch(() => {
          displayAlert("Game does not exist", "danger");
        });
    }

    joinGame();
    console.log("Joining the game...");
    setNavigatingToProperPage(true);
  
  }, [joiningGame]);

  useEffect(() => {
    if (!navigatingToProperPage) {
      return;
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

    navigateToProperPage();
    console.log("Navigating to the page...");

  }, [navigatingToProperPage]);

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
        <Popup 
          title={"Join the lobby"}
          inputFormPlaceholderText={"Paste the invitation hash here"}
          visible={popupVisible}
          onSubmit={handleOnSubmitPopup}
          onClose={handleClosePopup}
        />
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