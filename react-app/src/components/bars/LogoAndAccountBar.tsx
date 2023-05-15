import { BsPencilFill } from "react-icons/bs";
import Account from "../Account";

function LogoAndAccountBar() {
  return (
    <div className="row mb-3">
      <div className="col-12 col-md-3"></div>
      <div className="col-12 col-md-6">
        <h1 className="text-success text-center mt-3">
          <b>Scribbly <BsPencilFill/></b>
        </h1>
      </div>
      <div className="col-12 col-md-3 mt-3">
        <div className="d-flex justify-content-center justify-content-md-end">
          <Account />
        </div>
      </div>
    </div>
  )
}

export default LogoAndAccountBar;