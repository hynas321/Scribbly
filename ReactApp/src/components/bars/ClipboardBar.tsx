import { useState } from "react";
import CopyToClipboard from "react-copy-to-clipboard";
import { BsClipboard, BsEmojiSmile } from "react-icons/bs";
import Button from "../Button";
import { animated, useSpring } from "@react-spring/web";

interface ClipboardBarProps {
  invitationUrl: string;
}

function ClipboardBar({ invitationUrl }: ClipboardBarProps) {
  const [copiedToClipboardVisible, setcopiedToClipboardVisible] = useState(false);

  const clipboardBarAnimationSpring = useSpring({
    from: { x: -200 },
    to: { x: 0 },
  });

  const handleCopy = () => {
    setcopiedToClipboardVisible(true);

    setTimeout(() => {
      setcopiedToClipboardVisible(false);
    }, 1500);
  };

  return (
    <animated.div style={{ ...clipboardBarAnimationSpring }}>
      <CopyToClipboard text={invitationUrl} onCopy={handleCopy}>
        <Button text={"Copy invitation URL"} active={true} icon={<BsClipboard />} />
      </CopyToClipboard>
      {copiedToClipboardVisible && (
        <h4 className="text-success mt-3">
          Copied! <BsEmojiSmile />
        </h4>
      )}
    </animated.div>
  );
}

export default ClipboardBar;
