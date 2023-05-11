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
import useLocalStorageState from "use-local-storage-state";

function Canvas() {
  const hub = useContext(ConnectionHubContext);
  const dispatch = useDispatch();

  const player = useAppSelector((state) => state.player);
  const gameState = useAppSelector((state) => state.gameState);
  const gameSettings = useAppSelector((state) => state.gameSettings);

  const [color, setColor] = useState<string>("#000000");
  const [canvasTitle, setCanvasTitle] = useState<AnnouncementMessage | null>(null);
  const [isPlayerDrawing, setIsPlayerDrawing] = useState<boolean>(false);

  const [token, setToken] = useLocalStorageState("token", { defaultValue: "" });

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

  return (
    <>
      {
        gameState.isTimerVisible ?
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
        :
        canvasTitle && <h5 className={`text-${canvasTitle.bootstrapBackgroundColor}`}>{canvasTitle.text}</h5>
      }
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
      {
        player.username == gameState.drawingPlayerUsername &&
        <>
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
      }
    </>
  )
}

export default Canvas;