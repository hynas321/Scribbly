import { useContext, useEffect, useRef, useState } from 'react';
import ChatMessage, { ChatMessageProps } from './ChatMessage';
import InputForm from './InputForm';
import Button from './Button';
import { useAppSelector } from '../redux/hooks';
import { LobbyHubContext } from '../../Context/LobbyHubContext';

interface ChatProps {
  placeholderValue: string;
  wordLength?: number;
}

function Chat({placeholderValue, wordLength}: ChatProps) {
  const hub = useContext(LobbyHubContext);
  const username = useAppSelector((state) => state.player.username);

  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [inputFormValue, setInputFormValue] = useState("");
  const [activeButton, setActiveButton] = useState(false);
  const inputFormRef = useRef<HTMLInputElement>(null);
  const messagesRef = useRef<HTMLDivElement>(null);

  const testLobbyUrl = "TestLobbyUrl"; //temporary
  let characters: any[] = [];

  if (wordLength) {
    const character = '_';
    characters = new Array(wordLength).fill(character);
  }

  const handleButtonPress = () => {
    const chatMessage: ChatMessage = {
      username: username,
      text: inputFormValue
    }

    const SendChatMessage = async () => {
      await hub.invoke("SendChatMessage", testLobbyUrl, chatMessage.username, chatMessage.text);
    }

    if (chatMessage.username == "" || chatMessage.text == "") {
      return;
    }

    if (inputFormRef && inputFormRef.current) {
      inputFormRef.current.value = "";
    }

    SendChatMessage();
  }

  const handleEnterPress = (value: string, key: string) => {
    if (key == "Enter") {
      handleButtonPress();
    }
  }

  const handleInputFormChange = (value: string) => {
    setInputFormValue(value);
  }

  useEffect(() => {
    const getChatMessages = async () => {
      await hub.invoke("GetChatMessages", testLobbyUrl);
    };
  
    getChatMessages();
  }, []);

  useEffect(() => {
    if (inputFormValue.length > 0) {
      setActiveButton(true);
    } 
    else {
      setActiveButton(false);
    }
  }, [inputFormValue]);

  useEffect(() => {
    if (!messagesRef.current) {
      return;
    }

    messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
  }, [messages]);

  useEffect(() => {
    hub.on("ReceiveChatMessages", (chatMessageListSerialized: any) => {
      const chatMessageList = JSON.parse(chatMessageListSerialized) as ChatMessage[];

      setMessages(chatMessageList);
    });

    return() => {
      hub.off("ReceiveChatMessages");
    }
  }, [hub]);

  return (
    <div>
      <h5>
        {characters.map((c, index) => <span key={index}>{c} </span>)}
        { wordLength &&`${wordLength}` }
      </h5>
      <div id="messages" className="p-3 bg-light">
        <div ref={messagesRef} style={{height: "450px", overflowY: "auto"}}>
          {messages.map((chatMessage, index) => (
            <ChatMessage
              key={index}
              chatMessage={chatMessage}
            />
          ))}
        </div>
      </div>
      <div className="d-flex justify-content-center align-items-center">
        <Button
          text={"Send"}
          active={activeButton}
          onClick={handleButtonPress}
        />
        <InputForm 
          placeholderValue={placeholderValue}
          onChange={handleInputFormChange}
          onKeyDown={handleEnterPress}
          ref={inputFormRef}
        />
      </div>
    </div>
  );
}

export default Chat;