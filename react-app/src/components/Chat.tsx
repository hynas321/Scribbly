import { useEffect, useRef, useState } from 'react';
import ChatMessage, { ChatMessageProps } from './ChatMessage';
import InputForm from './InputForm';
import Button from './Button';
import { useAppSelector } from '../redux/hooks';

interface ChatProps {
  wordLength: number;
}

function Chat({wordLength}: ChatProps) {
  const username = useAppSelector((state) => state.player.username);
  const [messages, setMessages] = useState<ChatMessageProps[]>([]);
  const [inputFormValue, setInputFormValue] = useState("");
  const [activeButton, setActiveButton] = useState(false);
  const inputFormRef = useRef<HTMLInputElement>(null);
  const messagesRef = useRef<HTMLDivElement>(null);

  const character = '_';
  const characters = new Array(wordLength).fill(character);

  const handleButtonPress = () => {
    const chatMessageProp: ChatMessageProps = {
      username: username,
      text: inputFormValue
    }

    if (chatMessageProp.username == "" || chatMessageProp.text == "") {
      return;
    }

    setMessages(messages.concat(chatMessageProp))
    setInputFormValue("");

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

  return (
    <div>
      <h5>
        {characters.map(c => <text key={c}>{c} </text>)}
        {`${wordLength}`}
      </h5>
      <div id="messages" className="p-3 bg-light">
        <div ref={messagesRef} style={{height: "400px", overflowY: "auto"}}>
          {messages.map((chatMessageProp, index) => (
            <ChatMessage
              key={index}
              username={chatMessageProp.username}
              text={chatMessageProp.text}
            />
          ))}
        </div>
      </div>
      <div className="input-group justify-content-center">
        <Button
          text={"Send"}
          active={activeButton}
          onClick={handleButtonPress}
        />
        <InputForm 
          placeholderValue={"Enter your guess"}
          onChange={handleInputFormChange}
          onKeyDown={handleEnterPress}
          ref={inputFormRef}
        />
      </div>
    </div>
  );
}

export default Chat;