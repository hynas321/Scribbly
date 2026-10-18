import TextInput from "../forms/TextInput";
import Button from "../Button";
import MainScoreboard from "../MainScoreboard";
import PlayerScoreList from "../game-view-components/player-score/PlayerScoreList";
import tableLoading from "./../../assets/table-loading.gif";
import { animated, useSpring } from "@react-spring/web";
import config from "../../../config.json";
import { useJoinGameState } from "../../hooks/useJoinGameState";
import { useNavigate, useParams } from "react-router-dom";
import { useTopScores } from "../../hooks/useTopScores";
import { useTranslation } from "../../i18n/I18nContext";
import { useEffect, useState } from "react";
import api from "../../http/Api";
import { PlayerScore } from "../../interfaces/PlayerScore";

const JoinGameView = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { gameHash = "" } = useParams();
  const { scoreboardScores, isTableDisplayed } = useTopScores();
  const {
    username,
    setUsername,
    isJoinGameButtonActive,
    handleJoinGame,
    minUsernameLength,
    maxUsernameLength,
  } = useJoinGameState();
  const [lobbyPlayers, setLobbyPlayers] = useState<string[]>([]);
  const [lobbyMaxPlayers, setLobbyMaxPlayers] = useState(0);

  useEffect(() => {
    let active = true;
    api
      .fetchGamePlayers(gameHash)
      .then((lobby) => {
        if (!active) {
          return;
        }
        setLobbyPlayers(lobby.usernames);
        setLobbyMaxPlayers(lobby.maxPlayers);
      })
      .catch(() => undefined);
    return () => {
      active = false;
    };
  }, [gameHash]);

  const normalizedUsername = username.trim().toLowerCase();
  const isUsernameTaken =
    normalizedUsername.length > 0 &&
    lobbyPlayers.some((name) => name.trim().toLowerCase() === normalizedUsername);

  const animationSpring = useSpring({
    from: { y: 200 },
    to: { y: 0 },
  });

  return (
    <div className="container">
      <div className="col-lg-4 col-sm-7 col-xs-6 mx-auto text-center">
        <TextInput
          value={username}
          placeholderValue={t("common.usernamePlaceholder")}
          smallTextValue={
            isUsernameTaken
              ? t("joinGameView.usernameTaken")
              : t("common.usernameLengthHint", { min: minUsernameLength, max: maxUsernameLength })
          }
          onChange={setUsername}
        />
        <Button
          text={t("mainView.joinGame")}
          active={isJoinGameButtonActive && !isUsernameTaken}
          type={isUsernameTaken ? "secondary" : undefined}
          onClick={handleJoinGame}
        />
        <Button
          text={t("joinGameView.toMainPage")}
          active={true}
          type="secondary"
          onClick={() => navigate(config.mainClientEndpoint)}
        />
        {lobbyPlayers.length > 0 && (
          <div className="mt-4">
            <PlayerScoreList
              title={t("lobby.playersInLobby", {
                count: lobbyPlayers.length,
                max: lobbyMaxPlayers,
              })}
              playerScores={lobbyPlayers.map((name): PlayerScore => ({ username: name, score: 0 }))}
              displayPoints={false}
              displayIndex={false}
            />
          </div>
        )}
      </div>
      <animated.div
        className="col-lg-4 col-md-8 mt-5 text-center mx-auto"
        style={{ ...animationSpring }}
      >
        {isTableDisplayed ? (
          <MainScoreboard
            title={t("scoreboard.topPlayers")}
            scoreboardScores={scoreboardScores}
            displayPoints={true}
            displayIndex={true}
          />
        ) : (
          <img src={tableLoading} alt={t("scoreboard.loadingAlt")} className="img-fluid" />
        )}
      </animated.div>
    </div>
  );
};

export default JoinGameView;
