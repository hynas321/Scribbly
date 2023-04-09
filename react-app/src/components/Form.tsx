import { useEffect, useState } from 'react';

interface FormProps {
  placeholderValue: string;
  smallTextValue?: string;
  onChange: (value: string) => void;
}

function Form({placeholderValue, smallTextValue, onChange}: FormProps) {
  const [value, setValue] = useState("");

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setValue(event.target.value);
  }

  useEffect(() => {
    onChange(value);
  }, [value]);

  return (
    <div className="form-group mt-3">
        <input
          className="form-control"
          placeholder="Enter username"
          onChange={handleChange}
        />
        {placeholderValue && <small>{smallTextValue}</small>}
    </div>
  );
}

export default Form;