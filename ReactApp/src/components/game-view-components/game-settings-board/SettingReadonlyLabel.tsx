import { ReactNode } from "react";

interface SettingReadonlyLabelProps {
  label: string;
  children: ReactNode;
}

const SettingReadonlyLabel = ({ label, children }: SettingReadonlyLabelProps) => (
  <label className="form-check-label">
    {label}: <b>{children}</b>
  </label>
);

export default SettingReadonlyLabel;
