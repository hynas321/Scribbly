import { useContext, useEffect, useState } from "react";
import { useDraw } from "./hooks/useDraw";
import { CirclePicker } from "react-color"
import Button from "./Button";
import ProgressBar from "./ProgressBar";
import { GameHubContext } from "../Context/GameHubContext";

interface CanvasProps {
  progressBarProperties: ProgressProperties
}

function Canvas({progressBarProperties}: CanvasProps) {
  const hub = useContext(GameHubContext);
  const gameHash = "TestGameHash"; //temporary

  const [color, setColor] = useState("#000000");
  const { canvasRef, onMouseDown, clearCanvas } = useDraw(draw, hub, gameHash, color);

  function draw(canvasContext: CanvasRenderingContext2D, drawnLine: DrawnLine) {
    const {x: currentRelativeX, y: currentRelativeY} = drawnLine.currentPoint;
    const lineWidth = 5;

    let relativeStartPoint = drawnLine.previousPoint ?? drawnLine.currentPoint;
      
    canvasContext.beginPath();
    canvasContext.lineWidth = lineWidth;
    canvasContext.strokeStyle = drawnLine.color;
    canvasContext.moveTo(relativeStartPoint.x, relativeStartPoint.y);
    canvasContext.lineTo(currentRelativeX, currentRelativeY);
    canvasContext.stroke();
    canvasContext.fillStyle = drawnLine.color;
    canvasContext.beginPath();
    canvasContext.arc(relativeStartPoint.x, relativeStartPoint.y, 2, 0, 2 * Math.PI);
    canvasContext.fill();
  }

  return (
    <>
      <div className="d-flex justify-content-center">
        <div className="mb-3 col-10">
          <ProgressBar
            progressProperties={progressBarProperties}
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
        text={"Clear canvas"}
        active={true}
        onClick={clearCanvas}
      />
    </>
  )
}

export default Canvas;