import Button from "./Button";
import { BsDoorOpen } from "react-icons/bs";

interface ControlPanelProps {
  onClick: () => void;
}

function ControlPanel({onClick}: ControlPanelProps) {
  return (
    <>
      <Button
        text="Leave the game"
        active={true}
        type="danger"
        icon={<BsDoorOpen />}
        onClick={onClick}
      />
    </>
  )
}

export default ControlPanel;