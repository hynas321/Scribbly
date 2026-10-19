import Modal from "react-modal";
import Button from "./Button";
import TextInput from "./forms/TextInput";
import { useState } from "react";
import { useTranslation } from "../i18n/I18nContext";

interface PopupProps {
  title: string;
  inputFormPlaceholderText: string;
  visible: boolean;
  onSubmit: (value: string) => void;
  onClose: () => void;
}

function Popup({ title, inputFormPlaceholderText, visible, onSubmit, onClose }: PopupProps) {
  const { t } = useTranslation();
  const customStyles = {
    content: {
      top: "25%",
      left: "50%",
      right: "auto",
      bottom: "auto",
      transform: "translate(-50%, -50%)",
      width: "350px",
      borderRadius: "15px",
    },
  };

  const [inputFormValue, setInputFormValue] = useState("");
  const normalizedValue = inputFormValue.trim();
  const canSubmit = normalizedValue.length > 0;

  const close = () => {
    setInputFormValue("");
    onClose();
  };

  const submit = () => {
    if (!canSubmit) {
      return;
    }
    onSubmit(normalizedValue);
  };

  return (
    <Modal isOpen={visible} style={customStyles} ariaHideApp={false}>
      <div className="container">
        <h5>{title}</h5>
        <TextInput
          value={inputFormValue}
          placeholderValue={inputFormPlaceholderText}
          onChange={setInputFormValue}
          onKeyDown={(_, key) => {
            if (key === "Enter") {
              submit();
            }
          }}
        />
        <div className="d-flex justify-content-end">
          <Button text={t("common.cancel")} active={true} type={"danger"} onClick={close} />
          <Button text={t("common.submit")} active={canSubmit} type={"success"} onClick={submit} />
        </div>
      </div>
    </Modal>
  );
}

export default Popup;
