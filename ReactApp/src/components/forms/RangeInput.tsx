import { ChangeEvent, useId } from "react";

interface RangeInputProps {
  title: string;
  suffix: string;
  minValue: number;
  maxValue: number;
  step: number;
  value: number;
  onChange: (value: number) => void;
}

function RangeInput({ title, suffix, minValue, maxValue, step, value, onChange }: RangeInputProps) {
  const inputId = useId();
  const handleChange = (event: ChangeEvent<HTMLInputElement>) =>
    onChange(Number(event.target.value));

  return (
    <>
      <label className="form-label" htmlFor={inputId}>
        {`${title}: ${value}${suffix ? ` ${suffix}` : ""}`}
      </label>
      <input
        id={inputId}
        className="form-range d-block mx-auto"
        style={{ maxWidth: "14rem", width: "100%" }}
        type="range"
        value={value}
        min={minValue}
        max={maxValue}
        step={step}
        onChange={handleChange}
      />
    </>
  );
}

export default RangeInput;
