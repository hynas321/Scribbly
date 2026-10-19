import { toast } from "react-toastify";
import { ToastNotificationEnum } from "../enums/ToastNotificationEnum";
import { translate } from "../i18n/I18nContext";

export function reportRealtimeError(action: string) {
  toast.error(translate("error.realtime", { action }), {
    containerId: ToastNotificationEnum.Main,
    toastId: action,
  });
}
