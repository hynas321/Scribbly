import { useContext, useEffect, useRef, useState } from 'react';
import ChatMessage from '../ChatMessage';
import InputForm from '../InputForm';
import Button from '../Button';
import { ConnectionHubContext } from '../../context/ConnectionHubContext';
import * as signalR from '@microsoft/signalr';
import HubEvents from '../../hub/HubEvents';
import useLocalStorageState from 'use-local-storage-state';
import { useAppSelector } from '../../redux/hooks';
import { useDispatch } from 'react-redux';
import { updatedHiddenSecretWord } from '../../redux/slices/game-state-slice';
import UrlHelper from '../../utils/UrlHelper';

interface ChatProps {
  placeholderValue: string;
  displaySecretWord: boolean;
}

function Chat({placeholderValue, displaySecretWord}: ChatProps) {
  const hub = useContext(ConnectionHubContext);
  const dispatch = useDispatch();
  const player = useAppSelector((state) => state.player);
  const gameState = useAppSelector((state) => state.gameState);

  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [gameHash, setGameHash] = useState<string>("");
  const [inputFormValue, setInputFormValue] = useState<string>("");
  const [isSendButtonActive, setIsSendButtonActive] = useState<boolean>(false);

  const inputFormRef = useRef<HTMLInputElement>(null);
  const messagesRef = useRef<HTMLDivElement>(null);

  const [token, setToken] = useLocalStorageState("token", { defaultValue: "" });

  const handleButtonPress = () => {
    const SendChatMessage = async () => {
      if (inputFormValue.length < 1) {
        return;
      }

      await hub.invoke(HubEvents.sendChatMessage, gameHash, token, inputFormValue);
      
      if (inputFormRef && inputFormRef.current) {
        inputFormRef.current.value = "";
        setInputFormValue("");
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
    setGameHash(UrlHelper.getGameHash(window.location.href));
  }, []);

  useEffect(() => {
    if (hub.getState() !== signalR.HubConnectionState.Connected || !gameHash) {
      return;
    }

    hub.on(HubEvents.onLoadChatMessages, (chatMessagesSerialized: string) => {
      const chatMessageList = JSON.parse(chatMessagesSerialized) as ChatMessage[];
      setMessages(chatMessageList);
    });

    hub.on(HubEvents.onSendChatMessage, (chatMessageSerialized: string) => {
      const chatMessage = JSON.parse(chatMessageSerialized) as ChatMessage;
      setMessages(prevMessages => [...prevMessages, chatMessage]);
    });

    hub.on(HubEvents.onSendAnnouncement, (chatMessageSerialized: string) => {
      const chatMessage = JSON.parse(chatMessageSerialized) as ChatMessage;
      setMessages(prevMessages => [...prevMessages, chatMessage]);
    });

    hub.on(HubEvents.onRequestSecretWord, async () => {
      await hub.invoke(HubEvents.getSecretWord, gameHash, token);
    });

    hub.on(HubEvents.onGetSecretWord, (secretWord: string) => {
      dispatch(updatedHiddenSecretWord(secretWord));
    })

    const loadChatMessages = async () => {
      await hub.invoke(HubEvents.loadChatMessages, gameHash, token);
    };

    loadChatMessages();

    return () => {
      hub.off(HubEvents.onLoadChatMessages);
      hub.off(HubEvents.onSendChatMessage);
      hub.off(HubEvents.onSendAnnouncement);
    }
    }, [hub.getState(), gameHash]);

  useEffect(() => {
    if (inputFormValue.length > 0) {
      setIsSendButtonActive(true);
    } 
    else {
      setIsSendButtonActive(false);
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
        { displaySecretWord && `${gameState.hiddenSecretWord}`}
      </h5>
      <div id="messages" className="rounded-5 p-3 bg-light">
        <div ref={messagesRef} style={{height: "450px", overflowY: "auto"}}>
          {messages.map((chatMessage, index) => (
            <ChatMessage
              key={index}
              chatMessage={chatMessage}
            />
          ))}
        </div>
      </div>
      {
        (player.username != gameState.drawingPlayerUsername || gameState.drawingPlayerUsername == "") &&
        <div className="d-flex justify-content-center align-items-center">
          <Button
            text={"Send"}
            active={isSendButtonActive}
            onClick={handleButtonPress}
          />
          <InputForm 
            defaultValue={""} 
            placeholderValue={placeholderValue}
            onChange={handleInputFormChange}
            onKeyDown={handleEnterPress}
            ref={inputFormRef} 
          />
        </div>
      }

    </div>
  );
}

export default Chat;