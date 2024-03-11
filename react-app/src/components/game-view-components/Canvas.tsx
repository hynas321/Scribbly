import { useContext, useEffect, useState } from "react";
import { useDraw } from "../../hooks/useDraw";
import { CirclePicker } from "react-color"
import Button from "../Button";
import DrawingTimeBar from "../bars/DrawingTimeBar";
import { ConnectionHubContext } from "../../context/ConnectionHubContext";
import material from 'material-colors'
import HubEvents from "../../hub/HubEvents";
import * as signalR from '@microsoft/signalr';
import { useAppSelector } from "../../redux/hooks";
import { useDispatch } from "react-redux";
import { updatedCurrentDrawingTimeSeconds, updatedIsTimerVisible } from "../../redux/slices/game-state-slice";
import { BsArrowReturnLeft, BsEraserFill } from "react-icons/bs";
import Range from '../Range';
import { animated, useSpring } from "@react-spring/web";

function Canvas() {
  const hub = useContext(ConnectionHubContext);
  const dispatch = useDispatch();

  const player = useAppSelector((state) => state.player);
  const gameState = useAppSelector((state) => state.gameState);
  const gameSettings = useAppSelector((state) => state.gameSettings);

  const [color, setColor] = useState<string>("#000000");
  const [thickness, setThickness] = useState<number>(5);
  const [canvasTitle, setCanvasTitle] = useState<AnnouncementMessage | null>(null);
  const [isPlayerDrawing, setIsPlayerDrawing] = useState<boolean>(false);

  const { canvasRef, onMouseDown, clearCanvas, undoLine } = useDraw(draw, hub, color, thickness, isPlayerDrawing);

  const canvasAnimationSpring = useSpring({
    from: { y: 200 },
    to: { y: 0 },
  });

  const [canvasTitleAnimationSpring, setCanvasTitleAnimationSpring] = useSpring(() => ({ opacity: 0 }));
  const [, setCanvasToolsAnimationSpring] = useSpring(() => {{ 0 }});

  const circlePickerColors = [material.black, material.red['500'],
    material.pink['500'], material.purple['500'], material.deepPurple['500'],
    material.indigo['500'], material.blue['500'], material.green['500'],
    material.lightGreen['500'], material.yellow['500'], material.amber['500'],
    material.orange['500'], material.deepOrange['500'], material.brown['500'],
    material.blueGrey['500'], material.white
  ]

  function draw(canvasContext: CanvasRenderingContext2D, drawnLine: DrawnLine) {
    const {x: currentRelativeX, y: currentRelativeY} = drawnLine.currentPoint;

    let relativeStartPoint = drawnLine.previousPoint ?? drawnLine.currentPoint;
      
    canvasContext.beginPath();
    canvasContext.lineWidth = drawnLine.thickness;
    canvasContext.lineCap = "round";
    canvasContext.strokeStyle = drawnLine.color;
    canvasContext.moveTo(relativeStartPoint.x, relativeStartPoint.y);
    canvasContext.lineTo(currentRelativeX, currentRelativeY);
    canvasContext.stroke();
  }

  const handleThicknessRangeChange = (thickness: number) => {
    setThickness(thickness);
  };

  useEffect(() => {
    if (hub.getState() !== signalR.HubConnectionState.Connected) {
      return;
    }

    hub.on(HubEvents.onUpdateTimer, (time: number) => {
      dispatch((updatedCurrentDrawingTimeSeconds(time)));
    })

    hub.on(HubEvents.onUpdateTimerVisibility, (visible: boolean) => {
      dispatch((updatedIsTimerVisible(visible)));
    });

    hub.on(HubEvents.onUpdateDrawingPlayer, (drawingPlayerUsername: string) => {
      if (drawingPlayerUsername == player.username) {
        setIsPlayerDrawing(true);
      }
      else {
        setIsPlayerDrawing(false);
      }
    });

    hub.on(HubEvents.OnSetCanvasText, (announcementMessageSerialized: string) => {
      const announcementMessage = JSON.parse(announcementMessageSerialized) as AnnouncementMessage;

      setCanvasTitle(announcementMessage);
    });

    return () => {
      hub.off(HubEvents.onUpdateTimer);
      hub.off(HubEvents.onUpdateTimerVisibility);
      hub.off(HubEvents.onUpdateDrawingPlayer);
      hub.off(HubEvents.OnSetCanvasText);
    }
  }, [hub.getState()]);

  useEffect(() => {
    setCanvasTitleAnimationSpring({
      opacity: 1,
      from: { opacity: 0 },
      config: { duration: 500 }
    });
  }, [canvasTitle]);

  useEffect(() => {
    if (player.username !== gameState.drawingPlayerUsername) {
      setCanvasToolsAnimationSpring({
        opacity: 1,
        from: { opacity: 0 }
      });
    }

  }, [gameState.drawingPlayerUsername]);

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
            <Button
              text={"Undo"}
              active={true}
              icon={<BsArrowReturnLeft />}
              onClick={undoLine}
            />
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
                onChange={handleThicknessRangeChange}
              />
            </div>
          </div>
        </>
      )}
    </animated.div>
  );
}

export default Canvas;