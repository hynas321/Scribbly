import { useEffect, useRef, useState } from "react";
import Hub from "../hub/Hub";
import HubEvents from "../hub/HubMessages";
import UrlHelper from "../utils/UrlHelper";
import * as signalR from "@microsoft/signalr";
import { DrawnLine } from "../interfaces/DrawnLine";
import { Point } from "../interfaces/Point";
import { useSessionStorage } from "./useSessionStorage";

export const useDraw = (
  onDraw: (canvasContext: CanvasRenderingContext2D, line: DrawnLine) => void,
    hub: Hub,
    color: string,
    thickness: number,
    isPlayerDrawing: boolean
  ) => {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const previousRelativePoint = useRef<Point | null>(null);

  const [gameHash, setGameHash] = useState<string>("");
  const [currentLineNumber, setCurrentLineNumber] = useState<number>(0);
  const [isMouseDown, setIsMouseDown] = useState<boolean>(false);

  const { authorizationToken } = useSessionStorage();

  const onMouseDown = () => {
    if (!isPlayerDrawing) {
      return;
    }

    setCurrentLineNumber(currentLineNumber + 1);
    setIsMouseDown(true);
  };

  const clearCanvas = async () => {
    if (!isPlayerDrawing) {
      return;
    }

    await hub.invoke(HubEvents.clearCanvas, gameHash, authorizationToken);
  };

  const undoLine = async () => {
    if (!isPlayerDrawing) {
      return;
    }

    await hub.invoke(HubEvents.undoLine, gameHash, authorizationToken);
  };

  useEffect(() => {
    setGameHash(UrlHelper.getGameHash(window.location.href));
  }, []);

  useEffect(() => {
    if (hub.getState() !== signalR.HubConnectionState.Connected || !gameHash) {
      return;
    }

    const getCanvasContext = (): CanvasRenderingContext2D | null => {
      const canvas = canvasRef.current;

      if (!canvas) {
        return null;
      }

      return canvas.getContext("2d");
    };

    const canvasContext = getCanvasContext() as CanvasRenderingContext2D;

    hub.on(HubEvents.onLoadCanvas, (drawnLinesSerialized) => {
      const drawnLines = JSON.parse(drawnLinesSerialized) as DrawnLine[];

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

    hub.invoke(HubEvents.loadCanvas, gameHash);

    return () => {
      hub.off(HubEvents.onLoadCanvas);
      hub.off(HubEvents.onDrawOnCanvas);
      hub.off(HubEvents.onClearCanvas);
    };
  }, [hub.getState(), gameHash]);

  useEffect(() => {
    const handler = (event: MouseEvent) => {
      if (!isMouseDown) {
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
        currentLine: currentLineNumber,
      };

      previousRelativePoint.current = drawnLine.currentPoint;

      hub.invoke(
        HubEvents.drawOnCanvas,
        gameHash,
        authorizationToken,
        JSON.stringify(drawnLine)
      );
    };

    const determinePointRelativeCoordinates = (event: MouseEvent) => {
      const canvas = canvasRef.current;

      if (canvas == null) {
        return;
      }

      const rect = canvas.getBoundingClientRect();
      const x = event.clientX - rect.left;
      const y = event.clientY - rect.top;

      return { x, y };
    };

    const mouseUpHandler = () => {
      setIsMouseDown(false);
      previousRelativePoint.current = null;
    };

    canvasRef.current?.addEventListener("mousemove", handler);
    window.addEventListener("mouseup", mouseUpHandler);

    return () => {
      canvasRef.current?.removeEventListener("mousemove", handler);
      window.removeEventListener("mouseup", mouseUpHandler);
    };
  }, [onDraw]);

  return { canvasRef, onMouseDown, clearCanvas, undoLine };
};
