import { ReactNode } from "react";

interface ButtonProps {
  text: string;
  active: boolean;
  type?: string;
  icon?: ReactNode;
  onClick?: () => void;
}

function Button({text, active, type, icon, onClick}: ButtonProps) {
  return (
    <>
      <button 
        className={
          active ? `btn btn-${type == undefined ? "primary" : type} mt-3 mx-1` :
          `btn btn-${type == undefined ? "primary" : type} mt-3 mx-1 disabled`
        } 
        onClick={onClick}
      > 
        <span>{icon} {text}</span> 
      </button>
    </>
  );
}

export default Button;