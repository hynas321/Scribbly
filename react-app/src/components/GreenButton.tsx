interface GreenButtonProps {
  active: boolean;
  onClick: () => void;
}

function GreenButton({active, onClick}: GreenButtonProps) {

  return (
      <button 
        className={active ? "btn btn-success btn-lg mt-4" : "btn btn-success btn-lg mt-4 disabled"} 
        onClick={onClick}
      >
        Start the game
      </button>
  );
}

export default GreenButton