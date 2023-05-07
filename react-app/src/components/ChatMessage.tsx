import { useAppSelector } from "../redux/hooks";

export interface ChatMessageProps {
  chatMessage: ChatMessage
}

function ChatMessage({chatMessage}: ChatMessageProps) {
  const playerUsername = useAppSelector((state) => state.player.username);

  return (
    <div className={
      `bg-${chatMessage.bootstrapBackgroundColor != null ?
      chatMessage.bootstrapBackgroundColor : chatMessage.username == playerUsername ?
      "primary" :"info"}
      mb-1 px-2 py-1`}
    >
      <h6
        className={playerUsername == chatMessage.username ? "text-start text-warning" : "text-start text-white" }
        style={{overflowWrap: "break-word"}}
      >
          <b>{ chatMessage.username == null ? "" : `${chatMessage.username}: ` }</b>
          <span className={
            (playerUsername == chatMessage.username || chatMessage.username == null) ? "text-white" : "text-dark"}>{chatMessage.text}</span>
      </h6>
    </div>
  )
}

export default ChatMessage;