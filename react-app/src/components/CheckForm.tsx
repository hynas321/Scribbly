interface CheckFormProps {
  value: number;
  onChange: (value: number) => void;
}

function CheckForm() {
    return (
      <>
        <div className="form-check form-check-inline">
          <input className="form-check-input" type="radio" name="inlineRadioOptions" id="inlineRadio1" value="option1"/>
          <label className="form-check-label">1</label>
        </div>
        <div className="form-check form-check-inline">
          <input className="form-check-input" type="radio" name="inlineRadioOptions" id="inlineRadio2" value="option2"/>
          <label className="form-check-label">2</label>
        </div>
      </>
    )
}

export default CheckForm