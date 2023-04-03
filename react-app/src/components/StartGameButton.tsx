interface StartGameButtonProps {
  active: boolean;
  onClick: () => void;
}

function StartGameButton({active, onClick}: StartGameButtonProps) {

  return (
      <button 
        className={active ? "btn btn-success btn-lg mt-3" : "btn btn-success btn-lg mt-3 disabled"} 
        onClick={onClick}
      >
        Start the game
      </button>
  );
}

export default StartGameButton