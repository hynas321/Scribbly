export interface ChatMessageProps {
  chatMessage: ChatMessage
}

function ChatMessage({chatMessage}: ChatMessageProps) {
  return (
    <div className="bg-primary mb-1 px-2 py-1">
      <h6
        className="text-start text-warning"
        style={{overflowWrap: "break-word"}}
      >
        <b>{chatMessage.username}: </b><span className="text-white">{chatMessage.text}</span>
      </h6>
    </div>
  )
}

export default ChatMessage;