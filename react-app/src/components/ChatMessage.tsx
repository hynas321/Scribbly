export interface ChatMessageProps {
    username: string;
    text: string;
}

function ChatMessage({username, text}: ChatMessageProps) {
  return (
    <div className="bg-primary mb-1 px-2 py-1">
      <h6 className="text-start text-warning" style={{overflowWrap: "break-word"}}><b>{username}: </b><text className="text-white">{text}</text></h6>
    </div>
  )
}

export default ChatMessage;