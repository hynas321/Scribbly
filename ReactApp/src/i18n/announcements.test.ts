import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

const examples = [
  ["Alex joined the game", "Alex joined the game", "Alex dołączył do gry"],
  ["Alex left the game", "Alex left the game", "Alex opuścił grę"],
  [
    "Alex disconnected. Waiting for reconnect...",
    "Alex disconnected. Waiting for reconnect...",
    "Alex rozłączył się. Oczekiwanie na ponowne połączenie...",
  ],
  ["Round 12", "Round 12", "Runda 12"],
  [
    "Alex is going to draw in 5s",
    "Alex is going to draw in 5s",
    "Alex rozpocznie rysowanie za 5 s",
  ],
  ["The drawing phase has ended", "The drawing phase has ended", "Faza rysowania zakończona"],
  ["The answer was: żółty parasol", "The answer was: żółty parasol", "Hasło: żółty parasol"],
  [
    "Alex guessed the prompt (+125 points)",
    "Alex guessed the prompt (+125 points)",
    "Alex odgadł hasło (+125 pkt)",
  ],
  [
    "Alex received the drawing bonus (+30 points)",
    "Alex received the drawing bonus (+30 points)",
    "Alex otrzymał premię za rysowanie (+30 pkt)",
  ],
  [
    "Alex received no drawing bonus",
    "Alex received no drawing bonus",
    "Alex nie otrzymał premii za rysowanie",
  ],
  ["Thank you for playing!", "Thank you for playing!", "Dziękujemy za grę!"],
  [
    "Host left the game — the lobby was closed.",
    "The host left the game — the lobby was closed.",
    "Gospodarz opuścił grę — lobby zostało zamknięte.",
  ],
  [
    "Not enough players to continue the game",
    "Not enough players to continue the game.",
    "Zbyt mało graczy, by kontynuować grę.",
  ],
  ["Unknown system message", "Unknown system message", "Unknown system message"],
  ["The answer was: Round 1", "The answer was: Round 1", "Hasło: Round 1"],
  [
    "Żaneta {points} joined the game",
    "Żaneta {points} joined the game",
    "Żaneta {points} dołączył do gry",
  ],
] as const;

describe.each(["en", "pl"] as const)("system announcements in %s", (language) => {
  beforeEach(() => {
    vi.resetModules();
    vi.stubGlobal("localStorage", { getItem: () => language });
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it.each(examples)("%s", async (source, english, polish) => {
    const { translateAnnouncement } = await import("./I18nContext");
    expect(translateAnnouncement(source)).toBe(language === "pl" ? polish : english);
  });
});
