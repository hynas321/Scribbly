import { useEffect, useState } from "react";

interface RangeProps {
  title: string;
  minValue: number;
  maxValue: number;
  onChange: (value: number) => void;
}

function Range({title, minValue, maxValue, onChange}: RangeProps) {
  const [value, setValue] = useState(4);

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setValue(parseInt(event.target.value));
  }

  useEffect(() => {
    onChange(value)
  }, [value]);
  
  return (
    <div className="mt-4">
      <label className="form-label">{title}</label>
      <input 
        className="form-range"
        type="range"
        value={value}
        min={minValue}
        max={maxValue}
        onChange={handleChange}
      /> 
    </div>
  )
}

export default Range