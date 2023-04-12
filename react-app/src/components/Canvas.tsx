import { useState } from "react";
import { useDraw } from "./hooks/useDraw";
import { ChromePicker } from "react-color"

function Canvas() {
  const { canvasRef, onMouseDown } = useDraw(draw);
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
    <div className="d-flex">
      <canvas
        ref={canvasRef}
        width={500}
        height={500}
        className="border border-black rounded-md"
        onMouseDown={onMouseDown}
      />
      <div>
        <ChromePicker
          color={color}
          onChange={(e) => setColor(e.hex)}
        />
      </div>
    </div>
  )
}

export default Canvas;