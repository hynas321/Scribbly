import { animated, useSpring } from "@react-spring/web";
import Button from "../../Button";
import { BsDoorOpen } from "react-icons/bs";
import { useTranslation } from "../../../i18n/I18nContext";

interface ControlPanelProps {
  onClick: () => void;
}

const ControlPanel = ({ onClick }: ControlPanelProps) => {
  const { t } = useTranslation();
  const controlPanelAnimationSpring = useSpring({
    from: { x: -200 },
    to: { x: 0 },
  });

  return (
    <animated.div style={{ ...controlPanelAnimationSpring }}>
      <Button
        text={t("common.leaveGame")}
        active={true}
        type="danger"
        icon={<BsDoorOpen />}
        onClick={onClick}
      />
    </animated.div>
  );
};

export default ControlPanel;
