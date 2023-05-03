import { useContext, useEffect, useRef, useState } from 'react';
import ChatMessage from './ChatMessage';
import InputForm from './InputForm';
import Button from './Button';
import { useAppSelector } from '../redux/hooks';
import { ConnectionHubContext } from '../context/ConnectionHubContext';
import * as signalR from '@microsoft/signalr';
import HubEvents from '../hub/HubEvents';

interface ChatProps {
  placeholderValue: string;
  wordLength?: number;
}

function Chat({placeholderValue, wordLength}: ChatProps) {
  const hub = useContext(ConnectionHubContext);
  const player = useAppSelector((state) => state.player);

  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [inputFormValue, setInputFormValue] = useState("");
  const [activeButton, setActiveButton] = useState(false);
  const inputFormRef = useRef<HTMLInputElement>(null);
  const messagesRef = useRef<HTMLDivElement>(null);

  let characters: any[] = [];

  if (wordLength) {
    const character = '_';
    characters = new Array(wordLength).fill(character);
  }

  const handleButtonPress = () => {

    const SendChatMessage = async () => {
      await hub.invoke(HubEvents.sendChatMessage, player.token, inputFormValue);
      
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

    hub.on(HubEvents.onLoadChatMessages, (chatMessagesSerialized: any) => {
      const chatMessageList = JSON.parse(chatMessagesSerialized) as ChatMessage[];

      setMessages(chatMessageList);
    });

    const loadChatMessages = async () => {
      await hub.invoke(HubEvents.loadChatMessages, player.token);
    };

    loadChatMessages();

    return () => {
      hub.off(HubEvents.onLoadChatMessages);
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