// TypeScript equivalents of C# models from shared/Models.cs

export interface APIMessage {
  timestamp: string;
  temperature: number | null;
  humidity: number | null;
  currentSetpoint: number;
  isHeaterOn: boolean;
  overrideEnd: number | null;
  heaterOn: number | null;
  predictedTemperature: number | null;
  sensorFailures: number | null;
  overrideWithoutExpiry: boolean | null;
}

// Capitalized version matching C# API response
export interface APIMessageFromAPI {
  Timestamp: string;
  Temperature: number | null;
  Humidity: number | null;
  CurrentSetpoint: number;
  IsHeaterOn: boolean;
  OverrideEnd: number | null;
  HeaterOn: number | null;
  PredictedTemperature: number | null;
  SensorFailures: number | null;
  OverrideWithoutExpiry: boolean | null;
}

export interface SetPointMessage {
  setpoint: number;
  untilEpoch?: number | null;
  overrideWithoutExpiry: boolean;
}

export interface APIResponse<T> {
  status: number;
  payload: T;
}

// Weekly program: day (0=Monday) -> hour -> minute -> temperature
export type ProgramMessage = {
  [dayOfWeek: string]: {
    [hour: string]: {
      [minute: string]: number;
    };
  };
};

export interface ProgramEntry {
  day: number;
  hour: number;
  minute: number;
  temperature: number;
}

export const DAY_NAMES = [
  "Monday",
  "Tuesday",
  "Wednesday",
  "Thursday",
  "Friday",
  "Saturday",
  "Sunday",
] as const;

export const DURATION_OPTIONS = [
  { label: "1 hour", minutes: 60 },
  { label: "2 hours", minutes: 120 },
  { label: "4 hours", minutes: 240 },
  { label: "8 hours", minutes: 480 },
  { label: "Until cancelled", minutes: null },
] as const;
