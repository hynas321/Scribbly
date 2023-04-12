interface ButtonProps {
  text: string;
  active: boolean;
  type?: string;
  onClick?: () => void;
}

function Button({text, active, type, onClick}: ButtonProps) {
  return (
    <button 
      className={
        active ? `btn btn-${type == undefined ? "primary" : type} mt-3` :
        `btn btn-${type == undefined ? "primary" : type} mt-3 disabled`
      } 
      onClick={onClick}
    >
      {text}
    </button>
  );
}

export default Button;