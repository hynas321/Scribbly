import { Point } from "./Point"

export interface DrawnLine {
  currentPoint: Point,
  previousPoint: Point,
  color: string,
  thickness: number,
  currentLine: number
}