import { useEffect, useRef, useState } from "react"
import Hub from "../../Hubs/Hub";

export const useDraw = (onDraw: (
    canvasContext: CanvasRenderingContext2D,
    line: DrawnLine) => void,
  hub: Hub,
  hash: string,
  color: string) => {
  const [mouseDown, setMouseDown] = useState(false);
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const previousRelativePoint = useRef<Point | null>(null);

  const onMouseDown = () => {
    setMouseDown(true);
  }

  const clearCanvas = () => {
    hub.invoke("ClearCanvas", hash);
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

    hub.on("ApplyLoadCanvas", (drawnLinesSerialized) => {
      const drawnLines = JSON.parse(drawnLinesSerialized) as DrawnLine[];

      for (let i = 0; i < drawnLines.length; i++) {
        onDraw(canvasContext, drawnLines[i]);
      }
    });

    hub.on("UpdateCanvas", (drawnLineSerialized: string) => {
      const drawnLine = JSON.parse(drawnLineSerialized) as DrawnLine;

      onDraw(canvasContext, drawnLine);
    });

    hub.on("ApplyClearCanvas", () => {
      const canvas = canvasRef.current!;
      canvasContext.clearRect(0, 0, canvas.width, canvas.height);
    });

    hub.invoke("LoadCanvas", hash);

    return () => {
      hub.off("ApplyLoadCanvas");
      hub.off("UpdateCanvas");
      hub.off("ApplyClearCanvas");
    }
  }, [hub.getState()]);

  useEffect(() => {
    const handler = (event: MouseEvent) => {
      if (!mouseDown) {
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
        color: color
      }

      //onDraw(canvasContext, drawnLine);
      previousRelativePoint.current = drawnLine.currentPoint;
      
      hub.invoke("DrawOnCanvas", hash, JSON.stringify(drawnLine));
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

  return { canvasRef, onMouseDown, clearCanvas }
}