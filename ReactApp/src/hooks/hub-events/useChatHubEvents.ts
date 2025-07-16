import { useEffect } from "react";
import * as signalR from "@microsoft/signalr";
import Hub from "../../hub/Hub";
import { ChatMessage } from "../../interfaces/ChatMessage";
import { useDispatch } from "react-redux";
import { useSessionStorage } from "../useSessionStorage";
import HubEvents from "../../hub/HubMessages";
import { updatedHiddenSecretWord } from "../../redux/slices/game-state-slice";

export const useChatHubEvents = (
  hub: Hub,
  gameHash: string,
  setMessages: (messages: ChatMessage[] | ((prev: ChatMessage[]) => ChatMessage[])) => void
) => {
  const dispatch = useDispatch();
  const { authorizationToken } = useSessionStorage();

  useEffect(() => {
    if (hub.getState() !== signalR.HubConnectionState.Connected || !gameHash) {
      return;
    }

    hub.on(HubEvents.onLoadChatMessages, (chatMessagesSerialized: string) => {
      const chatMessageList = JSON.parse(chatMessagesSerialized) as ChatMessage[];
      setMessages(chatMessageList);
    });

    hub.on(HubEvents.onSendChatMessage, (chatMessageSerialized: string) => {
      const chatMessage = JSON.parse(chatMessageSerialized) as ChatMessage;
      setMessages((prev) => [...prev, chatMessage]);
    });

    hub.on(HubEvents.onSendAnnouncement, (chatMessageSerialized: string) => {
      const chatMessage = JSON.parse(chatMessageSerialized) as ChatMessage;
      setMessages((prev) => [...prev, chatMessage]);
    });

    hub.on(HubEvents.onRequestSecretWord, async () => {
      await hub.invoke(HubEvents.getSecretWord, gameHash, authorizationToken);
    });

    hub.on(HubEvents.onGetSecretWord, (secretWord: string) => {
      dispatch(updatedHiddenSecretWord(secretWord));
    });

    (async () => {
      await hub.invoke(HubEvents.loadChatMessages, gameHash, authorizationToken);
    })();

    return () => {
      hub.off(HubEvents.onLoadChatMessages);
      hub.off(HubEvents.onSendChatMessage);
      hub.off(HubEvents.onSendAnnouncement);
      hub.off(HubEvents.onRequestSecretWord);
      hub.off(HubEvents.onGetSecretWord);
    };
  }, [hub, gameHash, authorizationToken]);
};
