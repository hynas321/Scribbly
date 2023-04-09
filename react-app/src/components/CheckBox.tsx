import { useEffect, useState } from "react";

interface CheckBoxProps {
  text: string;
  checkedByDefault: boolean;
  onChange: (checked: boolean) => void;
}

function CheckBox({text, checkedByDefault, onChange}: CheckBoxProps) {
  const [checked, setChecked] = useState(checkedByDefault);

  const handleChange = (event: any) => {
    setChecked(event.target.checked);
  }

  useEffect(() => {
    onChange(checked)
  }, [checked]);

  return (
    <div className="form-check form-switch mt-3">
      <input 
        className="form-check-input"
        type="checkbox"
        checked={checked}
        onChange={handleChange}
      />
      <label className="form-check-label">{text}</label>
    </div>
    )
}

export default CheckBox;