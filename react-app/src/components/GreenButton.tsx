interface ButtonProps {
  text: string;
  active: boolean;
  type?: string;
  onClick?: () => void;
}

function Button({text, active, type, onClick}: ButtonProps) {

  return (
      <button 
        className={active ? `btn btn-${type == undefined ? "primary" : type} btn-lg mt-4` :
          `btn btn-${type == undefined ? "primary" : type} btn-lg mt-4 disabled`} 
        onClick={onClick}
      >
        {text}
      </button>
  );
}

export default Button;