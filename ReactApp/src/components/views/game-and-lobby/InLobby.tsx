import { useContext, useMemo } from "react";
import { BsPlayCircle, BsDoorOpen } from "react-icons/bs";
import ClipboardBar from "../../bars/ClipboardBar";
import Button from "../../Button";
import PlayerScores from "../../game-view-components/player-score/PlayerScores";
import GameSettingsBoard from "../../game-view-components/game-settings-board/GameSettingsBoard";
import Chat from "../../game-view-components/chat/Chat";
import { ConnectionHubContext } from "../../../context/ConnectionHubContext";
import { useAppSelector } from "../../../redux/hooks";
import { useLobbyActions } from "../../../hooks/useLobbyActions";
import { PlayerScore } from "../../../interfaces/PlayerScore";
import { useTranslation } from "../../../i18n/I18nContext";
import config from "../../../../config.json";

interface LobbyViewProps {
  playerScores: PlayerScore[];
  isPlayerHost: boolean;
  isStartGameButtonActive: boolean;
  onLeave: () => void;
  gameHash: string;
}

const InLobby = ({
  playerScores,
  isPlayerHost,
  isStartGameButtonActive,
  onLeave,
  gameHash,
}: LobbyViewProps) => {
  const { t } = useTranslation();
  const hub = useContext(ConnectionHubContext);
  const player = useAppSelector((state) => state.player);
  const settings = useAppSelector((state) => state.gameSettings);
  const { handleStartGame } = useLobbyActions(hub, gameHash, settings);
  const invitationUrl = useMemo(
    () => `${window.location.origin}${config.joinGameClientEndpoint}/${gameHash}`,
    [gameHash],
  );

  return (
    <div className="container mb-3">
      <div className="row">
        <div className="col-lg-3 col-sm-6 col-12 mx-auto mt-2 text-center order-lg-1 order-2 mb-3">
          <div className="col-lg-8 col-md-10 mx-auto">
            <PlayerScores
              title={t("lobby.playersInLobby", {
                count: playerScores.length,
                max: settings.maxPlayers,
              })}
              playerScores={playerScores}
              displayPoints={false}
              displayIndex={false}
              displayRound={false}
            />
            <ClipboardBar invitationUrl={invitationUrl} />
          </div>
        </div>
        <div className="col-lg-6 col-sm-10 col-12 mx-auto text-center order-lg-2 order-1">
          <h5>{t("lobby.yourUsername", { username: player.username })}</h5>
          {isPlayerHost ? (
            <Button
              text={t("lobby.startGame")}
              type="success"
              active={isStartGameButtonActive}
              icon={<BsPlayCircle />}
              onClick={handleStartGame}
            />
          ) : (
            <h4 className="mt-3">{t("lobby.waitingForHost")}</h4>
          )}
          <Button
            text={t("common.leaveGame")}
            active
            icon={<BsDoorOpen />}
            type="danger"
            onClick={onLeave}
          />
          <div className="mt-3">
            <GameSettingsBoard isPlayerHost={isPlayerHost} />
          </div>
        </div>
        <div className="col-lg-3 order-lg-3 col-sm-7 col-12 mx-auto order-3">
          <div className="col-12 mb-3">
            <Chat placeholderValue={t("chat.messagePlaceholder")} displayPrompt={false} />
          </div>
        </div>
      </div>
    </div>
  );
};
export default InLobby;
