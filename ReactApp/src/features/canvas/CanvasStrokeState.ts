import { CanvasSnapshot, CanvasStrokeChunk, CanvasStrokeSnapshot } from "../../hub/HubInterfaces";
import { drawStroke } from "./strokes";

export default class CanvasStrokeState {
  private currentEpoch = 1;
  private readonly strokes = new Map<string, CanvasStrokeSnapshot>();

  get epoch() {
    return this.currentEpoch;
  }

  applySnapshot(snapshot: CanvasSnapshot) {
    this.currentEpoch = snapshot.epoch;
    this.strokes.clear();

    snapshot.strokes.forEach((stroke) => {
      this.strokes.set(stroke.strokeId, {
        ...stroke,
        coordinates: [...stroke.coordinates],
      });
    });
  }

  appendChunk(chunk: CanvasStrokeChunk) {
    if (chunk.epoch !== this.currentEpoch) {
      return;
    }

    const existing = this.strokes.get(chunk.strokeId);

    if (existing) {
      existing.coordinates.push(...chunk.coordinates);
      return;
    }

    this.strokes.set(chunk.strokeId, {
      strokeId: chunk.strokeId,
      color: chunk.color,
      thickness: chunk.thickness,
      coordinates: [...chunk.coordinates],
    });
  }

  clear(epoch: number) {
    this.currentEpoch = epoch;
    this.strokes.clear();
  }

  remove(strokeId: string, epoch: number) {
    if (epoch === this.currentEpoch) {
      this.strokes.delete(strokeId);
    }
  }

  addPoint(strokeId: string, color: string, thickness: number, x: number, y: number) {
    const stroke = this.strokes.get(strokeId);

    if (stroke) {
      stroke.coordinates.push(x, y);
      return;
    }

    this.strokes.set(strokeId, {
      strokeId,
      color,
      thickness,
      coordinates: [x, y],
    });
  }

  keepOnlyCurrentEpoch(chunks: CanvasStrokeChunk[]) {
    return chunks.filter((chunk) => chunk.epoch === this.currentEpoch);
  }

  render(context: CanvasRenderingContext2D) {
    this.strokes.forEach((stroke) => drawStroke(context, stroke));
  }
}
