import { useEffect, useState } from "react";

interface CheckFormProps {
  title: string;
  radioCount: number;
  radioCheckedByDefault: number;
  onChange: (value: number) => void;
}

function CheckForm({title, radioCount, radioCheckedByDefault, onChange}: CheckFormProps) {
  const [checkedValue, setCheckedValue] = useState(radioCheckedByDefault);
  const radios = Array.from({length: radioCount}, (_, index) => index + 1);

  const handleChange = (event: any) => {
    setCheckedValue(event.target.value);
  }

  useEffect(() => {
    onChange(checkedValue);
  }, [checkedValue]);

  const radiosList = radios.map((num) => (
    <div className="form-check form-check-inline" key={num}>
      <input 
        className="form-check-input"
        type="radio"
        name="radioOption"
        value={num}
        onChange={handleChange}
        checked={num == checkedValue}
      />
      <label className="form-check-label">{num}</label>
    </div>
  ));

  return (
    <div className="mt-4">
      <div>
        <label className="form-label">{title}</label>
        <div>{radiosList}</div>
      </div>
    </div>
  )
}

export default CheckForm;