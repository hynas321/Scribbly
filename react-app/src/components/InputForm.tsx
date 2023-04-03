import { useEffect, useState } from 'react';

interface InputFormProps {
  placeholderValue: string;
  smallTextValue?: string;
  onChange: (value: string) => void;
}

function InputForm({placeholderValue, smallTextValue, onChange}: InputFormProps) {
  const [inputFormValue, setInputFormValue] = useState("");

  const handleInputFormChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setInputFormValue(event.target.value);
  }

  useEffect(() => {
    onChange(inputFormValue)
  }, [inputFormValue]);

  return (
    <div className="form-group mt-4">
        <input className="form-control" placeholder="Enter username" onChange={handleInputFormChange}></input>
        {placeholderValue && <small>{smallTextValue}</small>}
    </div>
  );
}

export default InputForm