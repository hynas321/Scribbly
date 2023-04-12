import { useEffect, useRef, useState } from "react"

export const useDraw = (onDraw: ({
    canvasContext,
    currentRelativePoint,
    previousRelativePoint
  }: Draw) => void) => {
  const [mouseDown, setMouseDown] = useState(false);
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const previousRelativePoint = useRef<Point | null>(null);
  
  const onMouseDown = () => {
    setMouseDown(true);
  }

  const clearCanvas = () => {
    const canvas = canvasRef.current;

    if (!canvas) {
      return;
    }

    const canvasContext = canvas.getContext("2d");
    
    if (!canvasContext) {
      return;
    }

    canvasContext.clearRect(0, 0, canvas.width, canvas.height);
  }

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

      onDraw({
        canvasContext,
        currentRelativePoint,
        previousRelativePoint: previousRelativePoint.current!
      });

      previousRelativePoint.current = currentRelativePoint;
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