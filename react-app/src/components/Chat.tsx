import { useContext, useEffect, useRef, useState } from 'react';
import ChatMessage, { ChatMessageProps } from './ChatMessage';
import InputForm from './InputForm';
import Button from './Button';
import { useAppSelector } from '../redux/hooks';
import { LobbyHubContext } from '../Context/LobbyHubContext';
import { GameHubContext } from '../Context/GameHubContext';
import { HubType } from '../enums/HubType';
import * as signalR from '@microsoft/signalr';

interface ChatProps {
  hubType: HubType
  placeholderValue: string;
  wordLength?: number;
}

function Chat({hubType, placeholderValue, wordLength}: ChatProps) {
  const hub = useContext(hubType === HubType.LOBBY ? LobbyHubContext : GameHubContext);
  const username = useAppSelector((state) => state.player.username);

  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [inputFormValue, setInputFormValue] = useState("");
  const [activeButton, setActiveButton] = useState(false);
  const inputFormRef = useRef<HTMLInputElement>(null);
  const messagesRef = useRef<HTMLDivElement>(null);

  const testHash = (hubType === HubType.LOBBY ? "TestLobbyHash" : "TestGameHash"); //temporary
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
      await hub.invoke("SendChatMessage", testHash, chatMessage);
      
      if (inputFormRef && inputFormRef.current) {
        inputFormRef.current.value = "";
      }
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

    if (hub.getState() != signalR.HubConnectionState.Connected) {
      return;
    }

    hub.on("ReceiveChatMessages", (chatMessageListSerialized: any) => {
      const chatMessageList = JSON.parse(chatMessageListSerialized) as ChatMessage[];

      setMessages(chatMessageList);
    });

    const getChatMessages = async () => {
      await hub.invoke("GetChatMessages", testHash, username);
    };

    getChatMessages();

    return () => {
      hub.off("ReceiveChatMessages");
    }
    }, [hub.getState()]);

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