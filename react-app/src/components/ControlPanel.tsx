import { Link } from "react-router-dom";
import Button from "./Button";
import config from "./../../config.json"
import { BsDoorOpen } from "react-icons/bs";

function ControlPanel() {
  return (
    <>
      <Link to={config.mainClientEndpoint}>
        <Button
          text="Leave the game"
          active={true}
          type="danger"
          icon={<BsDoorOpen />}
        />
      </Link>
    </>
  )
}

export default ControlPanel;