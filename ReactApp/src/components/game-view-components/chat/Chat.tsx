import { useContext, useMemo, useState } from "react";
import { animated, useSpring } from "@react-spring/web";
import TextInput from "../../forms/TextInput";
import Button from "../../Button";
import ChatMessageList from "./ChatMessageList";
import { ConnectionHubContext } from "../../../context/ConnectionHubContext";
import HubEvents from "../../../hub/HubMessages";
import { useAppSelector } from "../../../redux/hooks";
import { useParams } from "react-router-dom";
import { reportRealtimeError } from "../../../utils/reportRealtimeError";
import { useRoomChat } from "../../../features/chat/useRoomChat";
import { translateCategory, useTranslation } from "../../../i18n/I18nContext";

const MAX_MESSAGE_LENGTH = 280;

interface ChatProps {
  placeholderValue: string;
  displayPrompt: boolean;
}

const Chat = ({ placeholderValue, displayPrompt }: ChatProps) => {
  const { t } = useTranslation();
  const hub = useContext(ConnectionHubContext);
  const player = useAppSelector((state) => state.player);
  const gameState = useAppSelector((state) => state.gameState);
  const { gameHash = "" } = useParams();
  const messages = useRoomChat(hub, gameHash);
  const [inputValue, setInputValue] = useState("");
  const animation = useSpring({ from: { x: 200 }, to: { x: 0 } });
  const canSend = useMemo(
    () => inputValue.trim().length > 0 && inputValue.length <= MAX_MESSAGE_LENGTH,
    [inputValue],
  );
  const send = async () => {
    const text = inputValue.trim();
    if (!text || text.length > MAX_MESSAGE_LENGTH) {
      return;
    }
    try {
      await hub.invoke(HubEvents.sendChatMessage, gameHash, text);
      setInputValue("");
    } catch {
      reportRealtimeError(t("error.action.sendMessage"));
    }
  };

  return (
    <animated.div style={{ ...animation }}>
      {displayPrompt && (
        <div className="mb-2">
          <h5 className="mb-1" style={{ letterSpacing: "0.12em" }}>
            {gameState.hiddenPromptText}
          </h5>
          {gameState.promptCategory && (
            <small className="text-muted">
              {t("prompt.categoryLabel")}: {translateCategory(gameState.promptCategory)}
            </small>
          )}
        </div>
      )}
      <ChatMessageList chatMessages={messages} />
      {(player.username !== gameState.drawingPlayerUsername ||
        gameState.drawingPlayerUsername === "") && (
        <div className="d-flex justify-content-center align-items-center">
          <Button
            text={t("chat.send")}
            type={canSend ? undefined : "secondary"}
            active={canSend}
            onClick={() => void send()}
          />
          <TextInput
            value={inputValue}
            placeholderValue={placeholderValue}
            onChange={setInputValue}
            onKeyDown={(_, key) => {
              if (key === "Enter") {
                void send();
              }
            }}
          />
        </div>
      )}
    </animated.div>
  );
};
export default Chat;
