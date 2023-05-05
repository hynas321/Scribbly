import { useContext, useState } from "react";
import { useDraw } from "../hooks/useDraw";
import { CirclePicker } from "react-color"
import Button from "./Button";
import DrawingTimeBar from "./bars/DrawingTimeBar";
import { ConnectionHubContext } from "../context/ConnectionHubContext";
import material from 'material-colors'

interface CanvasProps {
  progressBarProperties: ProgressProperties
}
function Canvas({progressBarProperties}: CanvasProps) {
  const hub = useContext(ConnectionHubContext);
  const [color, setColor] = useState("#000000");
  const { canvasRef, onMouseDown, clearCanvas } = useDraw(draw, hub, color);

  const circlePickerColors = [material.black, material.red['500'],
    material.pink['500'], material.purple['500'], material.deepPurple['500'],
    material.indigo['500'], material.blue['500'], material.lightBlue['500'],
    material.green['500'], material.lightGreen['500'], material.lime['500'],
    material.yellow['500'], material.amber['500'], material.orange['500'],
    material.deepOrange['500'], material.brown['500'], material.blueGrey['500']
  ]

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
          <DrawingTimeBar
            progressProperties={progressBarProperties}
            text="s"
          />
        </div>
      </div>
      <div className="d-flex justify-content-center mb-2">
        <canvas
          ref={canvasRef}
          width={700}
          height={500}
          className="border border-black rounded-md canvas-scale"
          onMouseDown={onMouseDown}
          onTouchStart={onMouseDown}
        />
      </div>
      <div className="d-flex justify-content-center">
        <CirclePicker
          color={color}
          width="100"
          onChange={(e) => setColor(e.hex)}
          colors={circlePickerColors}
        />
      </div>
      <div>
        <Button 
          text={"Clear canvas"}
          active={true}
          onClick={clearCanvas}
        />
      </div>
    </>
  )
}

export default Canvas;