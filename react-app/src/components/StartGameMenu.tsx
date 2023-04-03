import { useEffect, useState } from 'react';
import StartGameButton from './StartGameButton';
import InputForm from './InputForm';

function MainMenu() {
  const [username, setUsername] = useState("");
  const [activeButton, setActiveButton] = useState(false);
  const minUsernameLength: number = 5;

  const handleInputFormChange = (value: string) => {
    setUsername(value.trim());
  }

  const handleStartGameButtonClick = () => {
    fetch('http://localhost:5159/api/Game', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        HostUsername: username
      })
    })
      .then(data => {
        console.log('Response data:', data);
      })
      .catch(error => {
        console.error('Error:', error);
      });
  }

  useEffect(() => {
    if (username.length >= minUsernameLength) {
      setActiveButton(true);
    } 
    else {
      setActiveButton(false);
    }
  }, [username]);

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