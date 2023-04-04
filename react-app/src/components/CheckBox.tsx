import { useState } from "react";

interface CheckBoxProps {
  checkedByDefault: boolean;
  onChange: (checked: boolean) => void;
}

function CheckBox({checkedByDefault}: CheckBoxProps) {
  const [checked, setChecked] = useState(checkedByDefault);

  const handleChange = (event: any) => {
    setChecked(event.target.checked)
  }

  return (
    <div className="form-check form-switch mt-3">
      <input 
        className="form-check-input"
        type="checkbox"
        checked={checked}
        onChange={handleChange}
      />
      <label className="form-check-label"> Allow only non-abstract nouns</label>
    </div>
    )
}

export default CheckBox