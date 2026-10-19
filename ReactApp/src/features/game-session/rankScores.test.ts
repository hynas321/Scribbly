import { describe, expect, it } from "vitest";
import { rankScores } from "./rankScores";

describe("final standings", () => {
  it("sorts by points without changing the server result", () => {
    const scores = [
      { username: "Third", score: 10, isCurrentPlayer: false },
      { username: "Winner", score: 90, isCurrentPlayer: true },
      { username: "Second", score: 40, isCurrentPlayer: false },
    ];
    expect(rankScores(scores).map((player) => [player.username, player.place])).toEqual([
      ["Winner", 1],
      ["Second", 2],
      ["Third", 3],
    ]);
    expect(scores[0].username).toBe("Third");
    expect(rankScores(scores)[0].isCurrentPlayer).toBe(true);
  });

  it("shares places on ties and skips occupied places", () => {
    const scores = [100, 100, 50, 50, 0].map((score, index) => ({
      username: String(index),
      score,
      isCurrentPlayer: false,
    }));
    expect(rankScores(scores).map((player) => player.place)).toEqual([1, 1, 3, 3, 5]);
  });

  it("handles an empty result, one player, and a scoreless draw", () => {
    expect(rankScores([])).toEqual([]);
    expect(rankScores([{ username: "Solo", score: 0, isCurrentPlayer: true }])[0].place).toBe(1);
    expect(
      rankScores([
        { username: "A", score: 0, isCurrentPlayer: true },
        { username: "B", score: 0, isCurrentPlayer: false },
      ]).map((player) => player.place),
    ).toEqual([1, 1]);
  });
});
