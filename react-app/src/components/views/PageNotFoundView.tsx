import sadFace from '../../assets/sad-face.png';
import Button from '../Button';
import config from '../../../config.json';
import { Link } from 'react-router-dom';

function PageNotFoundView() {
  return (
    <div className="container text-center mt-3">
      <h3 className="mt-3">Page not found</h3>
      <div>
        <Link to={config.mainClientEndpoint}>
          <Button 
            text={"Go to main page"}
            active={true}
            type="danger"
          />
          </Link>
      </div>
      <img src={sadFace} alt="Logo" className="img-fluid"/>
    </div>
  )
}

export default PageNotFoundView;
