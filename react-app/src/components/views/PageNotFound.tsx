import sadFace from '../../assets/sad-face.png';
import Button from '../GreenButton';
import config from '../../../config.json';
import { Link } from 'react-router-dom';

function PageNotFound() {
  return (
    <div className="text-center mt-3">
      <h3 className="mt-3">Page not found</h3>
      <div>
        <Link to={config.createGameClientEndpoint}>
          <Button 
            text={"Go to main page"}
            active={true}
            type="danger"
          />
          </Link>
      </div>
      <img src={sadFace} alt="Logo" />
    </div>
  )
}

export default PageNotFound;
