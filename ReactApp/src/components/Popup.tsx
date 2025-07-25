import Modal from "react-modal";
import Button from "./Button";
import InputForm from "./InputForm";
import { useEffect, useState } from "react";

interface PopupProps {
  title: string;
  inputFormPlaceholderText: string;
  visible: boolean;
  onSubmit: (value: string) => void;
  onClose: () => void;
}

function Popup({ title, inputFormPlaceholderText, visible, onSubmit, onClose }: PopupProps) {
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
  const [joinLobbyActiveButton, setJoinLobbyActiveButton] = useState(false);

  const handleInputFormChange = (value: string) => {
    setInputFormValue(value);
  };

  useEffect(() => {
    if (inputFormValue.length != 0) {
      setJoinLobbyActiveButton(true);
    } else {
      setJoinLobbyActiveButton(false);
    }
  }, [inputFormValue]);

  return (
    <Modal isOpen={visible} style={customStyles} ariaHideApp={false}>
      <div className="container">
        <h5>{title}</h5>
        <InputForm placeholderValue={inputFormPlaceholderText} onChange={handleInputFormChange} />
        <div className="d-flex justify-content-end">
          <Button text={"Cancel"} active={true} type={"danger"} onClick={onClose} />
          <Button
            text={"Submit"}
            active={joinLobbyActiveButton}
            type={"success"}
            onClick={() => onSubmit(inputFormValue)}
          />
        </div>
      </div>
    </Modal>
  );
}

export default Popup;
