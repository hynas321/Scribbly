import { useState } from "react";
import CopyToClipboard from "react-copy-to-clipboard";
import { BsEmojiSmile, BsPersonAdd } from "react-icons/bs";
import Button from "./Button";

interface ClipboardBarProps {
  invitationUrl: string;
}

function ClipboardBar({invitationUrl}: ClipboardBarProps) {
  const [copiedToClipboardVisible, setcopiedToClipboardVisible] = useState(false);

  const handleCopy = () => {
    setcopiedToClipboardVisible(true);

    setTimeout(() => {
      setcopiedToClipboardVisible(false);
    }, 1500);
  }
  
  return (
    <>
      <div className="d-flex align-items-center">
        <h3><BsPersonAdd/></h3>
        <span className="mx-2"><b>{"Invitation link - share it to invite your friends!"}</b></span>
      </div>
      <CopyToClipboard 
        text={invitationUrl}
        onCopy={handleCopy}
      >
        <div className="d-flex align-items-center">
          <Button
            text={"Copy to clipboard"}
            active={true}
          />
        { copiedToClipboardVisible && <h4 className="text-success mx-5">Copied! <BsEmojiSmile /></h4>}
        </div>
      </CopyToClipboard>
    </>
  )
}

export default ClipboardBar;