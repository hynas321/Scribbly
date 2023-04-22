import { useEffect, useRef, useState } from 'react';
import ChatMessage, { ChatMessageProps } from './ChatMessage';
import InputForm from './InputForm';
import Button from './Button';
import { useAppSelector } from '../redux/hooks';
import Hub from './Hubs/Hub';

interface ChatProps {
  hub: Hub
  placeholderValue: string;
  wordLength?: number;
}

function Chat({hub, placeholderValue, wordLength}: ChatProps) {
  const username = useAppSelector((state) => state.player.username);
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
    const chatMessage: ChatMessage = {
      username: username,
      text: inputFormValue
    }

    if (chatMessage.username == "" || chatMessage.text == "") {
      return;
    }

    hub.invoke("SendChatMessage", chatMessage.text).then(() => console.log("sent")).catch((e) => console.log(e));

    if (inputFormRef && inputFormRef.current) {
      inputFormRef.current.value = "";
    }
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
    hub.on("ChatMessageReceived", (text) => {
      const chatMessage: ChatMessage = {
        username: username,
        text: text
      }

      console.log("Message delivered");
      setMessages(messages.concat(chatMessage))
    });

    return() => {
      hub.off("ChatMessageReceived");
    }
  }, []);

  return (
    <div>
      <h5>
        {characters.map((c, index) => <span key={index}>{c} </span>)}
        { wordLength &&`${wordLength}` }
      </h5>
      <div id="messages" className="p-3 bg-light">
        <div ref={messagesRef} style={{height: "400px", overflowY: "auto"}}>
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