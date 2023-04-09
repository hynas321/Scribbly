import { Link } from 'react-router-dom';
import DrawingBoard from '../DrawingBoard';
import Button from '../GreenButton';
import config from '../../../config.json'

function GameMenu() {
  return (
    <div className="text-center">
      <Link to={config.createGameClientEndpoint}>
        <Button
          text="Go back"
          type="danger"
          active={true}
          onClick={() => {}}
        />
      </Link>
    </div>
  )
}

export default GameMenu;