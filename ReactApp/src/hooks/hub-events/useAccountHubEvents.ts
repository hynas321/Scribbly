import { useEffect } from "react";
import * as signalR from "@microsoft/signalr";
import HubEvents from "../../hub/HubMessages";
import UrlHelper from "../../utils/UrlHelper";
import Hub from "../../hub/Hub";

export const useAccountHubEvents = (
  accountHub: Hub,
  authOAccountId: string,
  isUserLoggedIn: boolean,
  setIsScoreToBeUpdated: React.Dispatch<React.SetStateAction<boolean>>
) => {
  useEffect(() => {
    if (accountHub.getState() !== signalR.HubConnectionState.Connected) {
      return;
    }

    accountHub.on(HubEvents.onUpdateAccountScore, async (gameHash: string) => {
      if (!isUserLoggedIn) return;
      if (gameHash !== UrlHelper.getGameHash(window.location.href)) return;

      setIsScoreToBeUpdated(true);
    });

    accountHub.on(HubEvents.onSessionEnded, async () => {
      setIsScoreToBeUpdated(false);
    });

    return () => {
      accountHub.off(HubEvents.onUpdateAccountScore);
      accountHub.off(HubEvents.onSessionEnded);
    };
  }, [accountHub.getState(), authOAccountId]);
};
