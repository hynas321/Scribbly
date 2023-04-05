import { useEffect, useState } from 'react';
import GreenButton from '../GreenButton';
import Form from '../Form';
import GameSettingsBoard from '../GameSettingsBoard';
import Settings from '../../interfaces/Settings';

function StartGameMenu() {
  const minUsernameLength: number = 5;

  let settings: Settings = {
    nonAbstractNounsChecked: true,
    drawingTimespanSeconds: 75,
    roundsCount: 4
  };

  const [username, setUsername] = useState("");
  const [activeButton, setActiveButton] = useState(false);

  const handleInputFormChange = (value: string) => {
    setUsername(value.trim());
  }

  const handleSettingsChange = (settingsObj: Settings) => {
    settings = settingsObj;
    console.log(settings);
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
        <h1 className="text-success">Scribbly</h1>
        <Form
          placeholderValue="Enter username"
          smallTextValue={`Minimum username length ${minUsernameLength}`}
          onChange={handleInputFormChange}
        />
        <GreenButton
          active={activeButton}
          onClick={handleStartGameButtonClick}
        />
      </div>
      <div className="col-lg-5 col-md-7 col-sm-5 col-xs-5 mt-5 mx-auto">
        <GameSettingsBoard
          defaultSettings={settings}
          onChange={handleSettingsChange}
        />
      </div>
    </div>
  );
}

export default StartGameMenu