import { useEffect, useRef } from "react";
import { ChatMessage } from "../../../interfaces/ChatMessage";
import ChatMessageElement from "../../ChatMessage";

interface ChatMessageListProps {
  chatMessages: ChatMessage[];
};

const ChatMessageList = ({ chatMessages }: ChatMessageListProps) => {
  const messagesRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!messagesRef.current) {
      return;
    }

    messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
  }, [chatMessages]);

  return (
    <div id="messages" className="rounded-5 bg-light">
      <div ref={messagesRef} style={{ height: "450px", overflowY: "auto" }}>
        {chatMessages.map((chatMessage, index) => (
          <ChatMessageElement key={index} chatMessage={chatMessage} />
        ))}
      </div>
    </div>
  )
}

export default ChatMessageList;