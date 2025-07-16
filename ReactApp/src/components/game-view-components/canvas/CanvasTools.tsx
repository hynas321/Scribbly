import material from "material-colors";
import { CirclePicker } from "react-color";
import Button from "../../Button";
import Range from "../../Range";
import { BsArrowReturnLeft, BsEraserFill } from "react-icons/bs";

interface ChatProps {
  color: string;
  setColor: (c: string) => void;
  setThickness: (t: number) => void;
  undoLine: () => void;
  clearCanvas: () => void;
}

const CanvasTools = ({ color, setColor, setThickness, undoLine, clearCanvas }: ChatProps) => {
  const circlePickerColors = [
    material.black,
    material.red["500"],
    material.pink["500"],
    material.purple["500"],
    material.deepPurple["500"],
    material.indigo["500"],
    material.blue["500"],
    material.green["500"],
    material.lightGreen["500"],
    material.yellow["500"],
    material.amber["500"],
    material.orange["500"],
    material.deepOrange["500"],
    material.brown["500"],
    material.blueGrey["500"],
    material.white,
  ];

  return (
    <>
      <div className="custom-muted d-flex justify-content-center rounded py-2 px-3">
        <CirclePicker
          color={color}
          width="100"
          onChange={(e) => setColor(e.hex)}
          colors={circlePickerColors}
        />
      </div>
      <div className="d-flex justify-content-center">
        <Button text={"Undo"} active={true} icon={<BsArrowReturnLeft />} onClick={undoLine} />
        <Button
          text={"Clear canvas"}
          active={true}
          icon={<BsEraserFill />}
          onClick={clearCanvas}
        />
        <div className="mx-3">
          <Range
            title={"Thickness"}
            suffix={""}
            minValue={3}
            maxValue={30}
            step={3}
            defaultValue={3}
            onChange={setThickness}
          />
        </div>
      </div>
    </>
  )
}

export default CanvasTools;