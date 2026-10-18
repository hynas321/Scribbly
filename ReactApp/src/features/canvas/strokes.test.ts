import { describe, expect, it } from "vitest";
import { toCanvasPoint } from "./strokes";

describe("canvas coordinate packing", () => {
  it("maps viewport coordinates to intrinsic ushort-safe canvas coordinates", () => {
    expect(
      toCanvasPoint(110, 70, { left: 10, top: 20, width: 335, height: 235 }, 670, 470),
    ).toEqual([200, 100]);
  });

  it("clamps out-of-bounds pointers and ignores invisible canvas", () => {
    expect(
      toCanvasPoint(-20, 300, { left: 10, top: 20, width: 335, height: 235 }, 670, 470),
    ).toEqual([0, 470]);
    expect(toCanvasPoint(0, 0, { left: 0, top: 0, width: 0, height: 0 }, 670, 470)).toBeNull();
  });
});
