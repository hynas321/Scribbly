import { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";
import Hub from "../../hub/Hub";
import HubEvents from "../../hub/HubMessages";
import { ChatMessage } from "../../interfaces/ChatMessage";
import { AnnouncementMessage } from "../../interfaces/AnnouncementMessage";
import { useHubState } from "../../hooks/useHubState";

const MAX_VISIBLE_MESSAGES = 25;

export function useRoomChat(hub: Hub, gameHash: string) {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const hubState = useHubState(hub);

  useEffect(() => {
    if (!gameHash || hubState !== signalR.HubConnectionState.Connected) {
      return;
    }
    let active = true;
    const append = (message: ChatMessage) => {
      if (active) {
        setMessages((previous) => [...previous, message].slice(-MAX_VISIBLE_MESSAGES));
      }
    };
    const appendAnnouncement = (message: AnnouncementMessage) => {
      append({
        username: "",
        text: message.text,
        isAnnouncement: true,
        bootstrapBackgroundColor: message.bootstrapBackgroundColor,
      });
    };
    const disposers = [
      hub.on(HubEvents.onChatMessage, append),
      hub.on(HubEvents.onAnnouncement, appendAnnouncement),
    ];
    void hub
      .invoke<ChatMessage[]>(HubEvents.loadChatMessages, gameHash)
      .then((snapshot) => {
        if (active) {
          setMessages(snapshot.slice(-MAX_VISIBLE_MESSAGES));
        }
      })
      .catch(() => undefined);
    return () => {
      active = false;
      disposers.forEach((dispose) => dispose());
    };
  }, [hub, hubState, gameHash]);

  return messages;
}
