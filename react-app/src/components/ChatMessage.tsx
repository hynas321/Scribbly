import { useAppSelector } from "../redux/hooks";

export interface ChatMessageProps {
  chatMessage: ChatMessage
}

function ChatMessage({chatMessage}: ChatMessageProps) {
  const playerUsername = useAppSelector((state) => state.player.username);

  return (
    <div className={`bg-${chatMessage.bootstrapBackgroundColor ?? "primary"} mb-1 px-2 py-1 rounded-3`}>
      <h6
        className={playerUsername === chatMessage.username ? "text-start text-warning" : "text-start text-info"}
        style={{ overflowWrap: "break-word" }}
      >
        <b>{chatMessage.username == null ? "" : `${chatMessage.username}: `}</b>
        <span className={"text-white"}>{chatMessage.text}</span>
      </h6>
    </div>
  );
}

export default ChatMessage;