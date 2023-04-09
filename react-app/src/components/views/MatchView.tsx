import { Link } from 'react-router-dom';
import Button from '../Button';
import config from '../../../config.json';

function GameMenu() {
  return (
    <div className="text-center">
      <Link to={config.createGameClientEndpoint}>
        <Button
          text="Go back"
          active={true}
          type="danger"
        />
      </Link>
    </div>
  )
}

export default GameMenu;