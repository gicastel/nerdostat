import {
  APIMessage,
  APIMessageFromAPI,
  APIResponse,
  ProgramMessage,
  SetPointMessage,
} from '../types/models';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '';
const API_KEY = import.meta.env.VITE_API_KEY || '';

// Helper function to convert PascalCase API response to camelCase
function transformAPIMessage(apiData: APIMessageFromAPI): APIMessage {
  return {
    timestamp: apiData.Timestamp,
    temperature: apiData.Temperature,
    humidity: apiData.Humidity,
    currentSetpoint: apiData.CurrentSetpoint,
    isHeaterOn: apiData.IsHeaterOn,
    overrideEnd: apiData.OverrideEnd,
    heaterOn: apiData.HeaterOn,
    predictedTemperature: apiData.PredictedTemperature,
    sensorFailures: apiData.SensorFailures,
    overrideWithoutExpiry: apiData.OverrideWithoutExpiry,
  };
}

class APIClient {
  private baseUrl: string;
  private apiKey: string;

  constructor() {
    this.baseUrl = API_BASE_URL;
    this.apiKey = API_KEY;
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const url = `${this.baseUrl}/api/${endpoint}`;
    
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
      ...options.headers,
    };

    // Add API key to headers if available
    if (this.apiKey) {
      headers['x-functions-key'] = this.apiKey;
    }

    const response = await fetch(url, {
      ...options,
      headers,
    });

    if (!response.ok) {
      throw new Error(`API Error: ${response.status} ${response.statusText}`);
    }

    const data: APIResponse<T> = await response.json();
    return data.payload;
  }

  async getDeviceState(): Promise<APIMessage> {
    const rawData = await this.request<APIMessageFromAPI>('read');
    return transformAPIMessage(rawData);
  }

  async setManualSetpoint(
    temperature: number,
    durationMinutes: number | null,
    overrideWithoutExpiry: boolean = false
  ): Promise<APIMessage> {
    let untilEpoch: number | null = null;
    
    if (durationMinutes !== null) {
      // Calculate Unix epoch (in seconds)
      const futureTime = new Date(Date.now() + durationMinutes * 60 * 1000);
      untilEpoch = Math.floor(futureTime.getTime() / 1000);
    }

    const payload: SetPointMessage = {
      setpoint: temperature,
      untilEpoch,
      overrideWithoutExpiry,
    };

    const rawData = await this.request<APIMessageFromAPI>('setpoint/add', {
      method: 'POST',
      body: JSON.stringify(payload),
    });
    return transformAPIMessage(rawData);
  }

  async clearSetpoint(): Promise<APIMessage> {
    const rawData = await this.request<APIMessageFromAPI>('setpoint/clear', {
      method: 'POST',
    });
    return transformAPIMessage(rawData);
  }

  async setAwayMode(enabled: boolean): Promise<APIMessage> {
    const endpoint = enabled ? 'away/on' : 'away/off';
    const rawData = await this.request<APIMessageFromAPI>(endpoint, {
      method: 'POST',
    });
    return transformAPIMessage(rawData);
  }

  async getProgram(): Promise<ProgramMessage> {
    return this.request<ProgramMessage>('program');
  }

  async updateProgram(program: ProgramMessage): Promise<ProgramMessage> {
    return this.request<ProgramMessage>('program', {
      method: 'POST',
      body: JSON.stringify(program),
    });
  }
}

export const api = new APIClient();
