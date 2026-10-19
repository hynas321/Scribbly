import { PointerEvent as ReactPointerEvent, useCallback, useEffect, useRef } from "react";
import * as signalR from "@microsoft/signalr";
import { toast } from "react-toastify";
import Hub from "../../hub/Hub";
import HubEvents from "../../hub/HubMessages";
import { CanvasCommandResult, CanvasSnapshot, CanvasStrokeChunk } from "../../hub/HubInterfaces";
import { useHubState } from "../../hooks/useHubState";
import { ToastNotificationEnum } from "../../enums/ToastNotificationEnum";
import { translate, TranslationKey } from "../../i18n/I18nContext";
import {
  FLUSH_INTERVAL_MS,
  MAX_POINTS_PER_CHUNK,
  SAMPLE_INTERVAL_MS,
  toCanvasPoint,
} from "./strokes";
import CanvasStrokeState from "./CanvasStrokeState";

interface ActiveStroke {
  strokeId: string;
  sequence: number;
  color: string;
  thickness: number;
  pendingCoordinates: number[];
  lastSampleAt: number;
}

const canvasError = (key: TranslationKey) =>
  toast.error(translate(key), {
    containerId: ToastNotificationEnum.Main,
    toastId: "canvas-sync-error",
  });

export function useRealtimeCanvas(
  hub: Hub,
  gameHash: string,
  color: string,
  thickness: number,
  isPlayerDrawing: boolean,
) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const hubState = useHubState(hub);
  const strokeStateRef = useRef(new CanvasStrokeState());
  const pointerRef = useRef<number | null>(null);
  const activeStrokeRef = useRef<ActiveStroke | null>(null);
  const queueRef = useRef<CanvasStrokeChunk[]>([]);
  const processingRef = useRef(false);
  const retryTimerRef = useRef<number | null>(null);
  const processQueueRef = useRef<() => Promise<void>>(async () => undefined);

  const renderAll = useCallback(() => {
    const canvas = canvasRef.current;
    const context = canvas?.getContext("2d");

    if (!canvas || !context) {
      return;
    }
    context.clearRect(0, 0, canvas.width, canvas.height);
    strokeStateRef.current.render(context);
  }, []);

  const applySnapshot = useCallback(
    (snapshot: CanvasSnapshot) => {
      strokeStateRef.current.applySnapshot(snapshot);
      renderAll();
    },
    [renderAll],
  );

  const reloadSnapshot = useCallback(async () => {
    if (!gameHash || hub.getState() !== signalR.HubConnectionState.Connected) {
      return;
    }
    applySnapshot(await hub.invoke<CanvasSnapshot>(HubEvents.loadCanvas, gameHash));
  }, [hub, gameHash, applySnapshot]);

  const scheduleRetry = useCallback(() => {
    if (retryTimerRef.current !== null) {
      return;
    }
    retryTimerRef.current = window.setTimeout(() => {
      retryTimerRef.current = null;
      void processQueueRef.current();
    }, 750);
  }, []);

  const processQueue = useCallback(async () => {
    if (processingRef.current || hub.getState() !== signalR.HubConnectionState.Connected) {
      return;
    }
    processingRef.current = true;
    try {
      while (
        queueRef.current.length > 0 &&
        hub.getState() === signalR.HubConnectionState.Connected
      ) {
        const chunk = queueRef.current[0];
        try {
          const result = await hub.invoke<CanvasCommandResult>(
            HubEvents.appendCanvasChunk,
            gameHash,
            chunk,
          );

          if (result.status === "applied" || result.status === "duplicate") {
            queueRef.current.shift();
            continue;
          }

          if (result.status === "waitingForPlayers") {
            scheduleRetry();
            return;
          }

          if (result.status === "wrongEpoch" || result.status === "outOfOrder") {
            queueRef.current = [];
            activeStrokeRef.current = null;

            await reloadSnapshot();
            return;
          }

          if (result.status === "limitExceeded") {
            queueRef.current = [];
            canvasError("canvas.limitReached");
            return;
          }
          queueRef.current.shift();
        } catch {
          scheduleRetry();
          return;
        }
      }
    } finally {
      processingRef.current = false;
    }
  }, [hub, gameHash, reloadSnapshot, scheduleRetry]);
  processQueueRef.current = processQueue;

  const flushPending = useCallback((startProcessing = true) => {
    const stroke = activeStrokeRef.current;

    if (!stroke || stroke.pendingCoordinates.length === 0) {
      return;
    }
    const maxCoordinates = MAX_POINTS_PER_CHUNK * 2;
    while (stroke.pendingCoordinates.length > 0) {
      queueRef.current.push({
        epoch: strokeStateRef.current.epoch,
        strokeId: stroke.strokeId,
        sequence: stroke.sequence++,
        color: stroke.color,
        thickness: stroke.thickness,
        coordinates: stroke.pendingCoordinates.splice(0, maxCoordinates),
      });
    }

    if (startProcessing) {
      void processQueueRef.current();
    }
  }, []);

  useEffect(() => {
    const timer = window.setInterval(() => flushPending(), FLUSH_INTERVAL_MS);
    return () => window.clearInterval(timer);
  }, [flushPending]);

  useEffect(() => {
    if (hubState !== signalR.HubConnectionState.Connected) {
      flushPending(false);
      pointerRef.current = null;
      activeStrokeRef.current = null;
      return;
    }
    let active = true;
    const disposers = [
      hub.on(HubEvents.onCanvasChunk, (chunk: CanvasStrokeChunk) => {
        if (!active || chunk.epoch !== strokeStateRef.current.epoch) {
          return;
        }
        strokeStateRef.current.appendChunk(chunk);
        renderAll();
      }),
      hub.on(HubEvents.onCanvasCleared, (epoch: number) => {
        if (!active) {
          return;
        }
        strokeStateRef.current.clear(epoch);
        queueRef.current = strokeStateRef.current.keepOnlyCurrentEpoch(queueRef.current);
        activeStrokeRef.current = null;
        pointerRef.current = null;
        renderAll();
      }),
      hub.on(HubEvents.onCanvasStrokeUndone, (strokeId: string, epoch: number) => {
        if (!active || epoch !== strokeStateRef.current.epoch) {
          return;
        }
        strokeStateRef.current.remove(strokeId, epoch);
        renderAll();
      }),
    ];
    void (async () => {
      try {
        await reloadSnapshot();

        await processQueueRef.current();

        await reloadSnapshot();
      } catch {
        canvasError("canvas.syncError");
      }
    })();
    return () => {
      active = false;
      disposers.forEach((dispose) => dispose());
    };
  }, [hub, hubState, reloadSnapshot, renderAll, flushPending]);

  const addLocalPoint = (x: number, y: number) => {
    const active = activeStrokeRef.current;

    if (!active) {
      return;
    }
    strokeStateRef.current.addPoint(active.strokeId, active.color, active.thickness, x, y);
    active.pendingCoordinates.push(x, y);
    renderAll();

    if (active.pendingCoordinates.length >= MAX_POINTS_PER_CHUNK * 2) {
      flushPending();
    }
  };

  const pointFromEvent = (event: ReactPointerEvent<HTMLCanvasElement>) =>
    toCanvasPoint(
      event.clientX,
      event.clientY,
      event.currentTarget.getBoundingClientRect(),
      event.currentTarget.width,
      event.currentTarget.height,
    );

  const onPointerDown = (event: ReactPointerEvent<HTMLCanvasElement>) => {
    if (
      !isPlayerDrawing ||
      hubState !== signalR.HubConnectionState.Connected ||
      event.button !== 0 ||
      pointerRef.current !== null
    ) {
      return;
    }
    const point = pointFromEvent(event);

    if (!point) {
      return;
    }
    pointerRef.current = event.pointerId;
    activeStrokeRef.current = {
      strokeId: crypto.randomUUID(),
      sequence: 0,
      color,
      thickness,
      pendingCoordinates: [],
      lastSampleAt: performance.now(),
    };
    event.currentTarget.setPointerCapture(event.pointerId);
    addLocalPoint(point[0], point[1]);
  };

  const onPointerMove = (event: ReactPointerEvent<HTMLCanvasElement>) => {
    const active = activeStrokeRef.current;

    if (!active || pointerRef.current !== event.pointerId || !isPlayerDrawing) {
      return;
    }
    const now = performance.now();

    if (now - active.lastSampleAt < SAMPLE_INTERVAL_MS) {
      return;
    }
    active.lastSampleAt = now;
    const point = pointFromEvent(event);

    if (point) {
      addLocalPoint(point[0], point[1]);
    }
  };

  const onPointerUp = (event: ReactPointerEvent<HTMLCanvasElement>) => {
    if (pointerRef.current !== event.pointerId) {
      return;
    }
    flushPending();
    pointerRef.current = null;
    activeStrokeRef.current = null;

    if (event.currentTarget.hasPointerCapture(event.pointerId)) {
      event.currentTarget.releasePointerCapture(event.pointerId);
    }
  };

  const clearCanvas = async () => {
    try {
      const result = await hub.invoke<CanvasCommandResult>(HubEvents.clearCanvas, gameHash);

      if (result.status !== "applied") {
        canvasError("canvas.clearError");
      }
    } catch {
      canvasError("canvas.clearError");
    }
  };

  const undoStroke = async () => {
    try {
      const result = await hub.invoke<CanvasCommandResult>(HubEvents.undoStroke, gameHash);

      if (result.status !== "applied" && result.status !== "nothingToUndo") {
        canvasError("canvas.undoError");
      }
    } catch {
      canvasError("canvas.undoError");
    }
  };

  useEffect(
    () => () => {
      if (retryTimerRef.current !== null) {
        window.clearTimeout(retryTimerRef.current);
      }
    },
    [],
  );

  return { canvasRef, onPointerDown, onPointerMove, onPointerUp, clearCanvas, undoStroke };
}
