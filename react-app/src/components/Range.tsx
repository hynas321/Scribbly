import { useEffect, useState } from "react";

interface RangeProps {
  title: string;
  minValue: number;
  maxValue: number;
  step: number;
  defaultValue: number;
  onChange: (value: number) => void;
}

function Range({title, minValue, maxValue, step, defaultValue, onChange}: RangeProps) {
  const [value, setValue] = useState(defaultValue);

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setValue(parseInt(event.target.value));
  }

  useEffect(() => {
    onChange(value)
  }, [value]);
  
  return (
    <div className="mt-4">
      <label className="form-label">{`${title}: ${value} seconds`}</label>
      <input 
        className="form-range"
        type="range"
        value={value}
        min={minValue}
        max={maxValue}
        step={step}
        onChange={handleChange}
      /> 
    </div>
  )
}

export default Range