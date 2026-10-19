import { ReactNode, createContext, useCallback, useContext, useMemo, useState } from "react";
import { en, TranslationKey } from "./translations/en";
import { pl } from "./translations/pl";

export type { TranslationKey };
export type Language = "en" | "pl";
type TranslationParams = Record<string, string | number>;
type TranslationValue = string | ((count: number) => string);

const dictionaries: Record<Language, Record<TranslationKey, TranslationValue>> = { en, pl };
const storageKey = "scribbly.uiLanguage";

const readStoredLanguage = (): Language => {
  try {
    const stored = localStorage.getItem(storageKey);
    return stored === "en" || stored === "pl" ? stored : "en";
  } catch {
    return "en";
  }
};

let activeLanguage: Language = readStoredLanguage();

const interpolate = (template: string, params?: TranslationParams) =>
  params
    ? template.replace(/\{(\w+)\}/g, (_, name) => String(params[name] ?? `{${name}}`))
    : template;

export function translate(key: TranslationKey, params?: TranslationParams): string {
  const value = dictionaries[activeLanguage][key] ?? en[key];
  if (typeof value === "function") {
    return value(Number(params?.count ?? 0));
  }
  return interpolate(value, params);
}

export function translateCategory(slug: string): string {
  const key = `category.${slug}` as TranslationKey;
  return key in en ? translate(key) : slug.charAt(0).toUpperCase() + slug.slice(1);
}

const announcementPatterns: Array<[RegExp, TranslationKey]> = [
  [/^(?<username>.*?) joined the game$/s, "announcement.joined"],
  [/^(?<username>.*?) left the game$/s, "announcement.left"],
  [/^(?<username>.*?) disconnected\. Waiting for reconnect\.\.\.$/s, "announcement.disconnected"],
  [/^Round (?<round>\d+)$/, "announcement.round"],
  [/^(?<username>.*?) is going to draw in (?<seconds>\d+)s$/s, "announcement.drawingNext"],
  [/^The drawing phase has ended$/, "announcement.drawingEnded"],
  [/^The answer was: (?<answer>.*)$/s, "announcement.answer"],
  [
    /^(?<username>.*?) guessed the prompt \(\+(?<points>\d+) points\)$/s,
    "announcement.correctGuess",
  ],
  [
    /^(?<username>.*?) received the drawing bonus \(\+(?<points>\d+) points\)$/s,
    "announcement.drawingBonus",
  ],
  [/^(?<username>.*?) received no drawing bonus$/s, "announcement.noDrawingBonus"],
  [/^Thank you for playing!$/, "announcement.finished"],
  [/^Host left the game.*$/s, "session.hostLeft"],
  [/^Not enough players to continue the game$/, "session.notEnoughPlayers"],
];

export function translateAnnouncement(text: string, t = translate): string {
  for (const [pattern, key] of announcementPatterns) {
    const match = pattern.exec(text);
    if (match) {
      return t(key, match.groups);
    }
  }
  return text;
}

interface I18nContextValue {
  language: Language;
  setLanguage: (language: Language) => void;
  t: (key: TranslationKey, params?: TranslationParams) => string;
}

const I18nContext = createContext<I18nContextValue>({
  language: activeLanguage,
  setLanguage: () => undefined,
  t: translate,
});

export function I18nProvider({ children }: { children: ReactNode }) {
  const [language, setLanguageState] = useState<Language>(activeLanguage);

  const setLanguage = useCallback((next: Language) => {
    activeLanguage = next;
    setLanguageState(next);
    try {
      localStorage.setItem(storageKey, next);
    } catch {}
  }, []);

  const value = useMemo(() => ({ language, setLanguage, t: translate }), [language, setLanguage]);

  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>;
}

export const useTranslation = () => useContext(I18nContext);
