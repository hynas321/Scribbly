import { useEffect, useRef, useState } from "react";

interface InputSelectProps {
    title: string;
    defaultValue: string;
    onChange: (value: string) => void;
}

function InputSelect({title, defaultValue, onChange}: InputSelectProps) {
  const [selectedValue, setSelectedValue] = useState(defaultValue);
  const ref = useRef<HTMLSelectElement>(null);

  const handleChange = (event: any) => {
    setSelectedValue(event.target.value);
  }

  useEffect(() => {
    onChange(selectedValue)
  }, [selectedValue]);
  
  return (
      <>
        <label className="form-label">{title}</label>
        <select className="form-select" ref={ref} onChange={handleChange}>
          <option value="en" selected>English</option>
          <option value="pl">Polish</option>
        </select>
      </>
  )
}

export default InputSelect;