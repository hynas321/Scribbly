interface GreenButtonProps {
  text: string;
  active: boolean;
  onClick: () => void;
}

function GreenButton({text, active, onClick}: GreenButtonProps) {

  return (
      <button 
        className={active ? "btn btn-success btn-lg mt-4" : "btn btn-success btn-lg mt-4 disabled"} 
        onClick={onClick}
      >
        {text}
      </button>
  );
}

export default GreenButton