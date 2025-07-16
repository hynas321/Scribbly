import { animated, useSpring } from "@react-spring/web";
import Button from "../../Button";
import { BsDoorOpen } from "react-icons/bs";

interface ControlPanelProps {
  onClick: () => void;
}

const ControlPanel = ({ onClick }: ControlPanelProps) => {
  const controlPanelAnimationSpring = useSpring({
    from: { x: -200 },
    to: { x: 0 },
  });

  return (
    <animated.div style={{ ...controlPanelAnimationSpring }}>
      <Button
        text="Leave the game"
        active={true}
        type="danger"
        icon={<BsDoorOpen />}
        onClick={onClick}
      />
    </animated.div>
  );
}

export default ControlPanel;
