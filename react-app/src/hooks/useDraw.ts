import { useEffect, useRef, useState } from "react"
import Hub from "../hub/Hub";
import HubEvents from "../hub/HubEvents";
import useLocalStorageState from "use-local-storage-state";

export const useDraw = (onDraw: (
    canvasContext: CanvasRenderingContext2D,
    line: DrawnLine) => void,
  hub: Hub,
  color: string,
  thickness: number) => {
  const [mouseDown, setMouseDown] = useState(false);
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const previousRelativePoint = useRef<Point | null>(null);
  const [token, setToken] = useLocalStorageState("token", { defaultValue: "" });
  const [currentLineNumber, setCurrentLineNumber] = useState<number>(0);

  const onMouseDown = () => {
    setCurrentLineNumber(currentLineNumber + 1);
    setMouseDown(true);
  }

  const clearCanvas = async () => {
    await hub.invoke(HubEvents.clearCanvas, token);
  }

  const undoLine = async () => {
    await hub.invoke(HubEvents.undoLine, token);
  }

  useEffect(() => {
    if (!hub.getState())
    {
      return;
    }

    const getCanvasContext = (): (CanvasRenderingContext2D | null) => {
      const canvas = canvasRef.current;
  
      if (!canvas) {
        return null;
      }
  
      return canvas.getContext("2d");
    }
  
    const canvasContext = getCanvasContext() as CanvasRenderingContext2D;

    hub.on(HubEvents.onLoadCanvas, (drawnLinesSerialized) => {
      const drawnLines = JSON.parse(drawnLinesSerialized) as DrawnLine[];
      console.log(drawnLinesSerialized);
      for (let i = 0; i < drawnLines.length; i++) {
        onDraw(canvasContext, drawnLines[i]);
      }
    });

    hub.on(HubEvents.onDrawOnCanvas, (drawnLineSerialized: string) => {
      const drawnLine = JSON.parse(drawnLineSerialized) as DrawnLine;

      onDraw(canvasContext, drawnLine);
    });

    hub.on(HubEvents.onClearCanvas, () => {
      const canvas = canvasRef.current!;
      canvasContext.clearRect(0, 0, canvas.width, canvas.height);
    });

    hub.invoke(HubEvents.loadCanvas, token);

    return () => {
      hub.off(HubEvents.onLoadCanvas);
      hub.off(HubEvents.onDrawOnCanvas);
      hub.off(HubEvents.onClearCanvas);
    }
  }, [hub.getState()]);

  useEffect(() => {
    const handler = (event: MouseEvent) => {
      if (!mouseDown) {
        console.log(currentLineNumber);
        return;
      }

      const currentRelativePoint = determinePointRelativeCoordinates(event);
      const canvasContext = canvasRef.current?.getContext("2d");

      if (currentRelativePoint == null || canvasContext == null) {
        return;
      }

      const drawnLine: DrawnLine = {
        currentPoint: currentRelativePoint,
        previousPoint: previousRelativePoint.current!,
        color: color,
        thickness: thickness,
        currentLine: currentLineNumber
      }

      previousRelativePoint.current = drawnLine.currentPoint;
      
      hub.invoke(HubEvents.drawOnCanvas, token, JSON.stringify(drawnLine));
    };

    const determinePointRelativeCoordinates = (event: MouseEvent) => {
      const canvas = canvasRef.current;

      if (canvas == null) {
        return;
      }

      const rect = canvas.getBoundingClientRect();
      const x = event.clientX - rect.left;
      const y = event.clientY - rect.top;

      return { x, y }
    }

    const mouseUpHandler = () => {
      setMouseDown(false);
      previousRelativePoint.current = null;
    }

    canvasRef.current?.addEventListener("mousemove", handler)
    window.addEventListener("mouseup", mouseUpHandler);

    return () => {
      canvasRef.current?.removeEventListener("mousemove", handler); 
      window.removeEventListener("mouseup", mouseUpHandler);
    }
  }, [onDraw])

  return { canvasRef, onMouseDown, clearCanvas, undoLine }
}