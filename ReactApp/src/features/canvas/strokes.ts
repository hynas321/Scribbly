import { CanvasStrokeSnapshot } from "../../hub/HubInterfaces";
import config from "../../../config.json";

export const MAX_POINTS_PER_CHUNK = config.canvas.maxPointsPerChunk;
export const SAMPLE_INTERVAL_MS = config.canvas.sampleIntervalMilliseconds;
export const FLUSH_INTERVAL_MS = config.canvas.flushIntervalMilliseconds;

export function toCanvasPoint(
  clientX: number,
  clientY: number,
  rect: Pick<DOMRect, "left" | "top" | "width" | "height">,
  canvasWidth: number,
  canvasHeight: number,
): [number, number] | null {
  if (rect.width <= 0 || rect.height <= 0) {
    return null;
  }
  const x = Math.round(
    Math.max(0, Math.min(canvasWidth, ((clientX - rect.left) * canvasWidth) / rect.width)),
  );
  const y = Math.round(
    Math.max(0, Math.min(canvasHeight, ((clientY - rect.top) * canvasHeight) / rect.height)),
  );
  return [x, y];
}

export function drawStroke(context: CanvasRenderingContext2D, stroke: CanvasStrokeSnapshot) {
  if (stroke.coordinates.length < 2) {
    return;
  }
  context.beginPath();
  context.lineWidth = stroke.thickness;
  context.lineCap = "round";
  context.lineJoin = "round";
  context.strokeStyle = stroke.color;
  context.moveTo(stroke.coordinates[0], stroke.coordinates[1]);
  if (stroke.coordinates.length === 2) {
    context.lineTo(stroke.coordinates[0] + 0.01, stroke.coordinates[1] + 0.01);
  } else {
    for (let index = 2; index + 1 < stroke.coordinates.length; index += 2) {
      context.lineTo(stroke.coordinates[index], stroke.coordinates[index + 1]);
    }
  }
  context.stroke();
}
