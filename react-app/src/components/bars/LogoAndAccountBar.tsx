import { BsPencilFill } from "react-icons/bs";
import Account from "../Account";

function LogoAndAccountBar() {
  return (
    <div className="row">
      <div className="col-3"></div>
      <div className="col-6">
        <h1 className="text-success text-center mt-3">
          <b>Scribbly <BsPencilFill/></b>
        </h1>
      </div>
      <div className="col-3 mt-3">
        <Account />
      </div>
    </div>
  )
}

export default LogoAndAccountBar;