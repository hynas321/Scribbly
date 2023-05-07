import { ForwardedRef, forwardRef, useEffect, useState } from 'react';

interface InputFormProps {
  placeholderValue: string;
  defaultValue?: string;
  smallTextValue?: string;
  onChange: (value: string) => void;
  onKeyDown?: (value: string, key: string) => void;
}

const InputForm = forwardRef((
    {defaultValue, placeholderValue, smallTextValue, onChange, onKeyDown}: InputFormProps,
    ref: ForwardedRef<HTMLInputElement>
  ) => {
  const [value, setValue] = useState(defaultValue ?? "");

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setValue(event.target.value);
  }

  const handleKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
    if (onKeyDown != null) {
      onKeyDown(value, event.key);
    }
  }

  useEffect(() => {
    onChange(value);
  }, [value]);

  return (
    <div className="form-group mt-3">
      {
        defaultValue ?
          <input
            value={value}
            className="form-control"
            placeholder={placeholderValue}
            onChange={handleChange}
            onKeyDown={handleKeyDown}
            ref={ref}
          />
        :
          <input
            className="form-control"
            placeholder={placeholderValue}
            onChange={handleChange}
            onKeyDown={handleKeyDown}
            ref={ref}
          />
      }
        {placeholderValue && <small>{smallTextValue}</small>}
    </div>
  );
});

export default InputForm;