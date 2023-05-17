import { useState } from "react";
import CopyToClipboard from "react-copy-to-clipboard";
import { BsClipboard, BsEmojiSmile, BsPersonAdd } from "react-icons/bs";
import Button from "../Button";

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
      <CopyToClipboard 
        text={invitationUrl}
        onCopy={handleCopy}
      >
        <div className="d-flex align-items-center">
          <Button
            text={"Copy invitation URL"}
            active={true}
            icon={<BsClipboard />}
          />
          { copiedToClipboardVisible && <h4 className="text-success mx-5">Copied! <BsEmojiSmile /></h4>}
        </div>
      </CopyToClipboard>
    </>
  )
}

export default ClipboardBar;