import { useContext, useEffect, useState } from "react";
import { useDraw } from "../../../hooks/useDraw";
import DrawingTimeBar from "../../bars/DrawingTimeBar";
import { ConnectionHubContext } from "../../../context/ConnectionHubContext";
import { useAppSelector } from "../../../redux/hooks";
import { animated, useSpring } from "@react-spring/web";
import { DrawnLine } from "../../../interfaces/DrawnLine";
import { AnnouncementMessage } from "../../../interfaces/AnnouncementMessage";
import CanvasTools from "./CanvasTools";
import { useCanvasHubEvents } from "../../../hooks/hub-events/useCanvasHubEvents";

const Canvas = () => {
  const hub = useContext(ConnectionHubContext);

  const player = useAppSelector((state) => state.player);
  const gameState = useAppSelector((state) => state.gameState);
  const gameSettings = useAppSelector((state) => state.gameSettings);

  const [color, setColor] = useState<string>("#000000");
  const [thickness, setThickness] = useState<number>(5);
  const [canvasTitle, setCanvasTitle] = useState<AnnouncementMessage | null>(null);
  const [isPlayerDrawing, setIsPlayerDrawing] = useState<boolean>(false);

  useCanvasHubEvents(hub, setCanvasTitle, setIsPlayerDrawing);

  const { canvasRef, onMouseDown, clearCanvas, undoLine } = useDraw(
    draw,
    hub,
    color,
    thickness,
    isPlayerDrawing
  );

  const canvasAnimationSpring = useSpring({ from: { y: 200 }, to: { y: 0 } });
  const [canvasTitleAnimationSpring, setCanvasTitleAnimationSpring] = useSpring(() => ({ opacity: 0 }));
  const [, setCanvasToolsAnimationSpring] = useSpring(() => {{ 0 }});

  useEffect(() => {
    setCanvasTitleAnimationSpring({
      opacity: 1,
      from: { opacity: 0 },
      config: { duration: 500 },
    });
  }, [canvasTitle]);

  useEffect(() => {
    if (player.username !== gameState.drawingPlayerUsername) {
      setCanvasToolsAnimationSpring({
        opacity: 1,
        from: { opacity: 0 },
      });
    }
  }, [gameState.drawingPlayerUsername]);

  function draw(canvasContext: CanvasRenderingContext2D, drawnLine: DrawnLine) {
    const { x: currentRelativeX, y: currentRelativeY } = drawnLine.currentPoint;

    let relativeStartPoint = drawnLine.previousPoint ?? drawnLine.currentPoint;

    canvasContext.beginPath();
    canvasContext.lineWidth = drawnLine.thickness;
    canvasContext.lineCap = "round";
    canvasContext.strokeStyle = drawnLine.color;
    canvasContext.moveTo(relativeStartPoint.x, relativeStartPoint.y);
    canvasContext.lineTo(currentRelativeX, currentRelativeY);
    canvasContext.stroke();
  }

  return (
    <animated.div style={{ ...canvasAnimationSpring }}>
      {gameState.isTimerVisible ? (
        <div className="d-flex justify-content-center">
          <div className="mb-3 col-10">
            <DrawingTimeBar
              currentProgress={gameState.currentDrawingTimeSeconds}
              minProgress={0}
              maxProgress={gameSettings.drawingTimeSeconds}
              text="s"
            />
          </div>
        </div>
      ) : (
        canvasTitle && (
          <animated.h5
            className={`text-${canvasTitle.bootstrapBackgroundColor}`}
            style={{ ...canvasTitleAnimationSpring }}
          >
            {canvasTitle.text}
          </animated.h5>
        )
      )}
      <div className="d-flex justify-content-center mb-2">
        <canvas
          ref={canvasRef}
          width={670}
          height={470}
          className="border border-black rounded-md canvas-scale img-responsive"
          onMouseDown={onMouseDown}
          onTouchStart={onMouseDown}
        />
      </div>
      {player.username === gameState.drawingPlayerUsername && (
        <CanvasTools
          color={color}
          setColor={setColor}
          setThickness={setThickness}
          undoLine={undoLine}
          clearCanvas={clearCanvas}/>
      )}
    </animated.div>
  );
}

export default Canvas;
