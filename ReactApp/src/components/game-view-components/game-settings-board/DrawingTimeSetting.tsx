import { useAppSelector } from "../../../redux/hooks";
import Range from "../../Range";

interface DrawingTimeSettingProps {
  isPlayerHost: boolean;
  onChange: (value: number) => void;
}

const DrawingTimeSetting = ({ isPlayerHost, onChange }: DrawingTimeSettingProps) => {
  const drawingTimeSeconds = useAppSelector((state) => state.gameSettings.drawingTimeSeconds);
  
  return (
    <div className="mt-4">
      {isPlayerHost ? (
        <Range
          title={"Drawing time"}
          suffix={"s"}
          minValue={30}
          maxValue={120}
          step={15}
          defaultValue={drawingTimeSeconds}
          onChange={onChange}
        />
      ) : (
        <label className="form-check-label">
          {"Drawing time"}: <b>{drawingTimeSeconds.valueOf().toString()}s</b>
        </label>
      )}
    </div>
  )
}

export default DrawingTimeSetting;