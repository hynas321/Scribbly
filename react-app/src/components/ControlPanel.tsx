import { Link } from "react-router-dom";
import Button from "./Button";
import config from "./../../config.json"

function ControlPanel() {
  return (
    <>
      <Link to={config.createGameClientEndpoint}>
        <Button
          text="Leave"
          active={true}
          type="danger"
        />
      </Link>
    </>
  )
}

export default ControlPanel;