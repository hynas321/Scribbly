export interface ChatMessageProps {
    username: string;
    text: string;
}

function ChatMessage({username, text}: ChatMessageProps) {
  return (
    <div className="bg-primary mb-1 px-2 py-1">
      <h6 className="text-start text-warning"><b>{username}</b></h6>
      <h6 className="text-start text-white">{text}</h6>
    </div>
  )
}

export default ChatMessage;