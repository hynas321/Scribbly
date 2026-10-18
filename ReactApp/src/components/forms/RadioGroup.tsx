import { ChangeEvent } from "react";

interface RadioGroupProps {
  title: string;
  minValue: number;
  count: number;
  value: number;
  onChange: (value: number) => void;
  name: string;
}

function RadioGroup({ title, minValue, count, value, onChange, name }: RadioGroupProps) {
  const options = Array.from({ length: count }, (_, index) => index + minValue);
  const handleChange = (event: ChangeEvent<HTMLInputElement>) =>
    onChange(Number(event.target.value));

  return (
    <fieldset>
      <legend className="form-label fs-6">{title}</legend>
      <div>
        {options.map((option) => (
          <div className="form-check form-check-inline" key={option}>
            <input
              className="form-check-input"
              type="radio"
              name={name}
              id={`${name}-${option}`}
              value={option}
              onChange={handleChange}
              checked={option === value}
            />
            <label className="form-check-label" htmlFor={`${name}-${option}`}>
              {option}
            </label>
          </div>
        ))}
      </div>
    </fieldset>
  );
}

export default RadioGroup;
