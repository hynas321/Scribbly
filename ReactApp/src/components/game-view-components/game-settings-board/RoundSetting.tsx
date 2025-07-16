import CheckForm from "../../CheckForm";
import { useAppSelector } from "../../../redux/hooks";
import { BsLayersFill } from "react-icons/bs";

interface RoundsSettingProps {
  isPlayerHost: boolean;
  onChange: (value: number) => void;
}

const RoundsSetting = ({ isPlayerHost, onChange }: RoundsSettingProps) => {
  const roundsCount = useAppSelector((state) => state.gameSettings.roundsCount);

  return (
    <div className="mt-4">
      {isPlayerHost ? (
        <CheckForm
          title={"Number of rounds"}
          radioCount={6}
          defaultValue={roundsCount}
          onChange={onChange}
        />
      ) : (
        <label className="form-check-label">
          Number of rounds: <b>{roundsCount}</b> <BsLayersFill />
        </label>
      )}
    </div>
  );
}

export default RoundsSetting;