type Draw = {
  canvasContext: CanvasRenderingContext2D,
  currentRelativePoint: Point,
  previousRelativePoint: Point
}

type Point = { x: number, y: number }