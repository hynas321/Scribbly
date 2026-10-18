import { CirclePicker } from "react-color";
import Button from "../../Button";
import RangeInput from "../../forms/RangeInput";
import { BsArrowReturnLeft, BsEraserFill } from "react-icons/bs";
import { useTranslation } from "../../../i18n/I18nContext";
import config from "../../../../config.json";

interface CanvasToolsProps {
  color: string;
  thickness: number;
  setColor: (color: string) => void;
  setThickness: (thickness: number) => void;
  undoStroke: () => void;
  clearCanvas: () => void;
}

const palette = [
  "#000000",
  "#f44336",
  "#e91e63",
  "#9c27b0",
  "#673ab7",
  "#3f51b5",
  "#2196f3",
  "#4caf50",
  "#8bc34a",
  "#ffeb3b",
  "#ffc107",
  "#ff9800",
  "#ff5722",
  "#795548",
  "#607d8b",
  "#ffffff",
];

const CanvasTools = ({
  color,
  thickness,
  setColor,
  setThickness,
  undoStroke,
  clearCanvas,
}: CanvasToolsProps) => {
  const { t } = useTranslation();
  return (
    <>
      <div className="custom-muted d-flex justify-content-center rounded py-2 px-3">
        <CirclePicker
          color={color}
          width="100"
          onChange={(value) => setColor(value.hex)}
          colors={palette}
        />
      </div>
      <div className="d-flex justify-content-center">
        <Button text={t("canvas.undo")} active icon={<BsArrowReturnLeft />} onClick={undoStroke} />
        <Button
          text={t("canvas.clearCanvas")}
          active
          icon={<BsEraserFill />}
          onClick={clearCanvas}
        />
        <div className="mx-3">
          <RangeInput
            title={t("canvas.thickness")}
            suffix=""
            minValue={config.canvas.minThickness}
            maxValue={config.canvas.maxThickness}
            step={config.canvas.thicknessStep}
            value={thickness}
            onChange={setThickness}
          />
        </div>
      </div>
    </>
  );
};
export default CanvasTools;
