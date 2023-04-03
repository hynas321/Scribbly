import { useEffect, useState } from 'react';
import StartGameButton from './StartGameButton';
import InputForm from './InputForm';

function MainMenu() {
  const [username, setUsername] = useState("");
  const [inputFormValue, setInputFormValue] = useState("");
  const [activeButton, setActiveButton] = useState(false);
  const minUsernameLength: number = 5;

  const handleInputFormChange = (value: string) => {
    setInputFormValue(value.trim());
  }

  const handleStartGameButtonClick = () => {
    //TODO
  }

  useEffect(() => {
    if (inputFormValue.length >= minUsernameLength) {
      setActiveButton(true);
    } 
    else {
      setActiveButton(false);
    }
  }, [inputFormValue]);

  return (
    <div className="container">
      <div className="col-lg-4 col-sm-5 col-xs-6 mx-auto mt-3 text-center">
        <h1>Scribbly</h1>
        <InputForm placeholderValue="Enter username" smallTextValue={`Minimum username length ${minUsernameLength}`} onChange={handleInputFormChange}/>
        <StartGameButton active={activeButton} onClick={handleStartGameButtonClick}/>
      </div>
    </div>
  );
}

export default MainMenu