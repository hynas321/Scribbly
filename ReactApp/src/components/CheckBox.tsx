import { useEffect, useState } from "react";

interface CheckBoxProps {
  text: string;
  defaultValue: boolean;
  onChange: (checked: boolean) => void;
}

function CheckBox({ text, defaultValue, onChange }: CheckBoxProps) {
  const [checked, setChecked] = useState(defaultValue);

  const handleChange = (event: any) => {
    setChecked(event.target.checked);
  };

  useEffect(() => {
    onChange(checked);
  }, [checked]);

  return (
    <div className="form-check form-switch">
      <input
        className="form-check-input"
        type="checkbox"
        checked={checked}
        onChange={handleChange}
      />
      <label className="form-check-label">{text}</label>
    </div>
  );
}

export default CheckBox;
