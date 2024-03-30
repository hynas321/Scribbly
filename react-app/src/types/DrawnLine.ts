import { Point } from "./Point"

export type DrawnLine = {
  currentPoint: Point,
  previousPoint: Point,
  color: string,
  thickness: number,
  currentLine: number
}