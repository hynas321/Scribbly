import { useAppSelector } from "../redux/hooks";
import { ChatMessage } from "../interfaces/ChatMessage";
import { translateAnnouncement, useTranslation } from "../i18n/I18nContext";

export interface ChatMessageProps {
  chatMessage: ChatMessage;
}

function ChatMessageElement({ chatMessage }: ChatMessageProps) {
  const { t } = useTranslation();
  const playerUsername = useAppSelector((state) => state.player.username);

  return (
    <div
      className={`bg-${chatMessage.bootstrapBackgroundColor ?? "primary"} mb-1 px-2 py-1 rounded-3`}
    >
      <h6
        className={
          playerUsername === chatMessage.username
            ? "text-start text-warning"
            : "text-start text-info"
        }
        style={{ overflowWrap: "break-word" }}
      >
        <b>{chatMessage.username ? `${chatMessage.username}: ` : ""}</b>
        <span className={"text-white"}>
          {chatMessage.isAnnouncement
            ? translateAnnouncement(chatMessage.text, t)
            : chatMessage.text}
        </span>
      </h6>
    </div>
  );
}

export default ChatMessageElement;
