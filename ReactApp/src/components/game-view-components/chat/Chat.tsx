import { useContext, useEffect, useRef, useState } from "react";
import InputForm from "../../InputForm";
import Button from "../../Button";
import { ConnectionHubContext } from "../../../context/ConnectionHubContext";
import HubEvents from "../../../hub/HubMessages";
import { useAppSelector } from "../../../redux/hooks";
import UrlHelper from "../../../utils/UrlHelper";
import { animated, useSpring } from "@react-spring/web";
import { ChatMessage } from "../../../interfaces/ChatMessage";
import { useSessionStorage } from "../../../hooks/useSessionStorage";
import { useChatHubEvents } from "../../../hooks/hub-events/useChatHubEvents";
import ChatMessageList from "./ChatMessageList";

interface ChatProps {
  placeholderValue: string;
  displaySecretWord: boolean;
}

const Chat = ({ placeholderValue, displaySecretWord }: ChatProps) => {
  const hub = useContext(ConnectionHubContext);
  const player = useAppSelector((state) => state.player);
  const gameState = useAppSelector((state) => state.gameState);

  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [gameHash, setGameHash] = useState<string>("");
  const [inputFormValue, setInputFormValue] = useState<string>("");
  const [isSendButtonActive, setIsSendButtonActive] = useState<boolean>(false);

  const inputFormRef = useRef<HTMLInputElement>(null);
  const messagesRef = useRef<HTMLDivElement>(null);
  const { authorizationToken } = useSessionStorage();

  const chatAnimationSpring = useSpring({
    from: { x: 200 },
    to: { x: 0 },
  });

  useChatHubEvents(hub, gameHash, setMessages);

  useEffect(() => {
    setGameHash(UrlHelper.getGameHash(window.location.href));
  }, []);

  useEffect(() => {
    setIsSendButtonActive(inputFormValue.length > 0);
  }, [inputFormValue]);

  const handleButtonPress = async () => {
    if (inputFormValue.trim().length < 1) return;

    await hub.invoke(HubEvents.sendChatMessage, gameHash, authorizationToken, inputFormValue);

    if (inputFormRef && inputFormRef.current) {
      inputFormRef.current.value = "";
      setInputFormValue("");
    }
  };

  const handleEnterPress = (key: string) => {
    if (key === "Enter") {
      handleButtonPress();
    }
  };

  return (
    <animated.div style={{ ...chatAnimationSpring }}>
      <h5>{displaySecretWord && `${gameState.hiddenSecretWord}`}</h5>
      <div id="messages" className="rounded-5 p-3 bg-light">
        <div ref={messagesRef} style={{ height: "450px", overflowY: "auto" }}>
          <ChatMessageList chatMessages={messages} />
        </div>
      </div>
      {(player.username !== gameState.drawingPlayerUsername ||
        gameState.drawingPlayerUsername === "") && (
        <div className="d-flex justify-content-center align-items-center">
          <Button text={"Send"} active={isSendButtonActive} onClick={handleButtonPress} />
          <InputForm
            defaultValue=""
            placeholderValue={placeholderValue}
            onChange={setInputFormValue}
            onKeyDown={(_, key) => handleEnterPress(key)}
            ref={inputFormRef}
          />
        </div>
      )}
    </animated.div>
  );
};

export default Chat;
