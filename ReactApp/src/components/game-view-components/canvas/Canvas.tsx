import { useContext, useEffect, useState } from "react";
import { animated, useSpring } from "@react-spring/web";
import DrawingTimeBar from "../../bars/DrawingTimeBar";
import { ConnectionHubContext } from "../../../context/ConnectionHubContext";
import { useAppSelector } from "../../../redux/hooks";
import { AnnouncementMessage } from "../../../interfaces/AnnouncementMessage";
import CanvasTools from "./CanvasTools";
import HubEvents from "../../../hub/HubMessages";
import { useParams } from "react-router-dom";
import { useRealtimeCanvas } from "../../../features/canvas/useRealtimeCanvas";
import { translateAnnouncement, useTranslation } from "../../../i18n/I18nContext";
import config from "../../../../config.json";

const Canvas = () => {
  const { t } = useTranslation();
  const hub = useContext(ConnectionHubContext);
  const { gameHash = "" } = useParams();
  const player = useAppSelector((state) => state.player);
  const gameState = useAppSelector((state) => state.gameState);
  const settings = useAppSelector((state) => state.gameSettings);
  const [color, setColor] = useState("#000000");
  const [thickness, setThickness] = useState(config.canvas.defaultThickness);
  const [canvasTitle, setCanvasTitle] = useState<AnnouncementMessage | null>(null);
  const canDraw = player.username === gameState.drawingPlayerUsername && gameState.isTimerVisible;
  const animation = useSpring({ from: { y: 200 }, to: { y: 0 } });
  const [titleAnimation, titleApi] = useSpring(() => ({ opacity: 0 }));
  const { canvasRef, onPointerDown, onPointerMove, onPointerUp, clearCanvas, undoStroke } =
    useRealtimeCanvas(hub, gameHash, color, thickness, canDraw);

  useEffect(
    () => hub.on(HubEvents.onCanvasText, (message: AnnouncementMessage) => setCanvasTitle(message)),
    [hub],
  );
  useEffect(() => {
    titleApi.start({ opacity: 1, from: { opacity: 0 }, config: { duration: 350 } });
  }, [canvasTitle, titleApi]);

  return (
    <animated.div style={{ ...animation }}>
      {gameState.isTimerVisible ? (
        <div className="d-flex justify-content-center">
          <div className="mb-3 col-10">
            <DrawingTimeBar
              currentProgress={gameState.currentDrawingTimeSeconds}
              minProgress={0}
              maxProgress={settings.drawingTimeSeconds}
              text="s"
            />
          </div>
        </div>
      ) : (
        canvasTitle && (
          <animated.h5
            className={`text-${canvasTitle.bootstrapBackgroundColor}`}
            style={{ ...titleAnimation }}
          >
            {translateAnnouncement(canvasTitle.text, t)}
          </animated.h5>
        )
      )}
      <div className="d-flex justify-content-center mb-2">
        <canvas
          ref={canvasRef}
          width={config.canvas.width}
          height={config.canvas.height}
          className="border border-black rounded-md canvas-scale img-responsive"
          onPointerDown={onPointerDown}
          onPointerMove={onPointerMove}
          onPointerUp={onPointerUp}
          onPointerCancel={onPointerUp}
          onLostPointerCapture={onPointerUp}
          style={{ touchAction: "none" }}
        />
      </div>
      {canDraw && (
        <CanvasTools
          color={color}
          thickness={thickness}
          setColor={setColor}
          setThickness={setThickness}
          undoStroke={() => void undoStroke()}
          clearCanvas={() => void clearCanvas()}
        />
      )}
    </animated.div>
  );
};
export default Canvas;
