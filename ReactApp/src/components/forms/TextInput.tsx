import { ChangeEvent, ForwardedRef, forwardRef, KeyboardEvent } from "react";

interface TextInputProps {
  placeholderValue: string;
  value: string;
  smallTextValue?: string;
  onChange: (value: string) => void;
  onKeyDown?: (value: string, key: string) => void;
}

const TextInput = forwardRef(function TextInput(
  { value, placeholderValue, smallTextValue, onChange, onKeyDown }: TextInputProps,
  ref: ForwardedRef<HTMLInputElement>,
) {
  const handleChange = (event: ChangeEvent<HTMLInputElement>) => onChange(event.target.value);
  const handleKeyDown = (event: KeyboardEvent<HTMLInputElement>) => onKeyDown?.(value, event.key);

  return (
    <div className="form-group mt-3">
      <input
        value={value}
        className="form-control"
        placeholder={placeholderValue}
        onChange={handleChange}
        onKeyDown={handleKeyDown}
        ref={ref}
      />
      {smallTextValue && <small>{smallTextValue}</small>}
    </div>
  );
});

export default TextInput;
