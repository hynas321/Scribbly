import { useState } from "react";
import { useDraw } from "./hooks/useDraw";
import { CirclePicker } from "react-color"
import Button from "./Button";
import ProgressBar from "./ProgressBar";

interface CanvasProps {
  currentProgress: number,
  minProgress: number,
  maxProgress: number
}

function Canvas({currentProgress, minProgress, maxProgress}: CanvasProps) {
  const { canvasRef, onMouseDown, clearCanvas } = useDraw(draw);
  const [color, setColor] = useState("#000000");

  function draw({
    canvasContext,
    currentRelativePoint,
    previousRelativePoint
  }: Draw) {
    const {x: currentRelativeX, y: currentRelativeY} = currentRelativePoint;
    const lineWidth = 5;

    let relativeStartPoint = previousRelativePoint ?? currentRelativePoint;

    canvasContext.beginPath();
    canvasContext.lineWidth = lineWidth;
    canvasContext.strokeStyle = color;
    canvasContext.moveTo(relativeStartPoint.x, relativeStartPoint.y);
    canvasContext.lineTo(currentRelativeX, currentRelativeY);
    canvasContext.stroke();
    canvasContext.fillStyle = color;
    canvasContext.beginPath();
    canvasContext.arc(relativeStartPoint.x, relativeStartPoint.y, 2, 0, 2 * Math.PI);
    canvasContext.fill();
  }

  return (
    <>
      <div className="d-flex justify-content-center">
        <div className="mb-3 col-10">
          <ProgressBar
            currentProgress={currentProgress}
            minProgress={minProgress}
            maxProgress={maxProgress}
            text="s"
          />
        </div>
      </div>
      <canvas
        ref={canvasRef}
        width={700}
        height={500}
        className="border border-black rounded-md"
        onMouseDown={onMouseDown}
      />
      <div>
      </div>
      <CirclePicker
        color={color}
        width="100"
        onChange={(e) => setColor(e.hex)}
      />
      <Button 
        text={"Clean canvas"}
        active={true}
        onClick={clearCanvas}
      />
    </>
  )
}

export default Canvas;