import { useEffect, useState } from "react";

interface CheckFormProps {
  title: string;
  radioCount: number;
  defaultValue: number;
  onChange: (value: number) => void;
}

function CheckForm({title, radioCount, defaultValue, onChange}: CheckFormProps) {
  const [checkedValue, setCheckedValue] = useState(defaultValue);
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
    <div>
      <label className="form-label">{title}</label>
      <div>{radiosList}</div>
    </div>
  )
}

export default CheckForm;