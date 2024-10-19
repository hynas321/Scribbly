import { useEffect, useRef, useState } from "react";

interface InputSelectProps {
  title: string;
  defaultValue: string;
  onChange: (value: string) => void;
}

function InputSelect({ title, defaultValue, onChange }: InputSelectProps) {
  const [selectedValue, setSelectedValue] = useState(defaultValue);

  const handleChange = (event: any) => {
    setSelectedValue(event.target.value);
  };

  useEffect(() => {
    onChange(selectedValue);
  }, [selectedValue]);

  return (
    <>
      <label className="form-label">{title}</label>
      <select className="form-select" value={selectedValue} onChange={handleChange}>
        <option value="en">English</option>
        <option value="pl">Polish</option>
      </select>
    </>
  );
}

export default InputSelect;
