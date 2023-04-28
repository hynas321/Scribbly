import { useContext, useEffect, useState } from "react";
import { useAppSelector } from "../../redux/hooks";
import { ConnectionHubContext } from "../../context/ConnectionHubContext";
import { Player } from "../../redux/slices/player-slice";
import HubEvents from "../../hub/HubEvents";
import GameView from "./GameView";
import LobbyView from "./LobbyView";

function Game() {
  const gameHub = useContext(ConnectionHubContext);
  const player = useAppSelector((state) => state.player);
  const [playerList, setPlayerList] = useState<Player[]>([]);
  const testGameHash = "TestGameHash"; //temporary
  const gameActive = true;
  
  useEffect(() => {
      const setConnectionHub = async () => {
  
        const getPlayerList = (playerListSerialized: any) => {
          const playerList = JSON.parse(playerListSerialized) as Player[];
  
          setPlayerList(playerList);
        }
        
        gameHub.on(HubEvents.onPlayerJoinedGame, getPlayerList);
        gameHub.on(HubEvents.onPlayerLeftGame, getPlayerList);
  
        await gameHub.start();
        await gameHub.invoke(HubEvents.joinGame, player.gameHash, player.username);
      }
  
      const clearBeforeUnload = () => {
        gameHub.off(HubEvents.onPlayerJoinedGame);
        gameHub.off(HubEvents.onPlayerLeftGame);
        gameHub.send(HubEvents.leaveGame, player.gameHash, player.username);
      }
  
      setConnectionHub();
  
      window.addEventListener("beforeunload", clearBeforeUnload);
  
      return () => {
        clearBeforeUnload();
        window.removeEventListener("beforeunload", clearBeforeUnload);
      }
    }, []);

  return (
    <>
      { gameActive ?
        <GameView
          players={playerList}
        /> :

        <LobbyView 
          players={playerList}
          isPlayerHost={false}
          invitationUrl={""} 
        />
        }
    </>
  )
}

export default Game;