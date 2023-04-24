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
    hub.on("ApplyClearCanvas", () => {
      const canvas = canvasRef.current;

      if (!canvas) {
        return;
      }
  
      const canvasContext = canvas.getContext("2d");
      
      if (!canvasContext) {
        return;
      }

      canvasContext.clearRect(0, 0, canvas.width, canvas.height);
    });

    hub.on("UpdateCanvas", (drawnLineSerialized: string) => {
      const drawnLine = JSON.parse(drawnLineSerialized) as DrawnLine;
      
      const canvas = canvasRef.current;

      if (!canvas) {
        return;
      }
  
      const canvasContext = canvas.getContext("2d");
      
      if (!canvasContext) {
        return;
      }

      onDraw(canvasContext, drawnLine);

      previousRelativePoint.current = drawnLine.currentPoint;
    });

    return () => {
      hub.off("ApplyClearCanvas");
      hub.off("UpdateCanvas")
    }
  }, []);

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
      //previousRelativePoint.current = drawnLine.currentPoint;
      
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