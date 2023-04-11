import { Link } from 'react-router-dom';
import Button from '../Button';
import config from '../../../config.json';
import Chat from '../Chat';
import PlayerList from '../PlayerList';
import DrawingBoard from '../DrawingBoard';

function MatchView() {
  return (
    <div className="container-fluid text-center">
      <div className="row">
        <div className="col-2">
          <PlayerList />
        </div>
        <div className="col-7">
          <DrawingBoard />
        </div>
        <div className="col-3">
          <Chat />
        </div>
        <div>
          <Link to={config.createGameClientEndpoint}>
            <Button
              text="Leave"
              active={true}
              type="danger"
            />
          </Link>
        </div>
      </div>
    </div>
  )
}

export default MatchView;