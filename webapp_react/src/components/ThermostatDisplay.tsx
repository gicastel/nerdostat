import { useDeviceState } from "../hooks/useDeviceState";
import { format, formatDistanceToNow } from "date-fns";
import "./ThermostatDisplay.css";

export function ThermostatDisplay() {
  const { data, isLoading, error, isRefetching } = useDeviceState();

  if (error) {
    return (
      <div className="thermostat-display error">
        <h2>Connection Error</h2>
        <p>{error.message}</p>
        <p className="hint">Check if the API is running and accessible</p>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="thermostat-display loading">
        <div className="spinner"></div>
        <p>Loading thermostat data...</p>
      </div>
    );
  }

  if (!data) {
    return null;
  }

  const hasOverride =
    data.overrideWithoutExpiry === true ||
    (data.overrideEnd !== null &&
      data.overrideEnd > Math.floor(Date.now() / 1000));
  const getOverrideExpiryLabel = (
    epochSeconds: number | null,
  ): string | null => {
    if (!epochSeconds) {
      return null;
    }

    const expiryDate = new Date(epochSeconds * 1000);
    return `Override expires at ${format(expiryDate, "MMM d, HH:mm")}`;
  };

  const getLastUpdateLabel = (timestamp: string): string => {
    const timestampMs = new Date(timestamp).getTime();
    const deltaMs = Number.isNaN(timestampMs)
      ? 0
      : Math.abs(Date.now() - timestampMs);

    if (deltaMs < 60_000) {
      return "Updated just now";
    }

    return `Updated ${formatDistanceToNow(new Date(timestamp), { addSuffix: true })}`;
  };

  return (
    <div className="thermostat-display">
      <div className="status-header">
        <div className={`heater-status ${data.isHeaterOn ? "on" : "off"}`}>
          <span className="status-icon">{data.isHeaterOn ? "🔥" : "❄️"}</span>
          <span className="status-text">
            Heater {data.isHeaterOn ? "ON" : "OFF"}
          </span>
        </div>
        <div className="status-actions">
          <div className="last-update-badge">
            {getLastUpdateLabel(data.timestamp)}
          </div>
          {isRefetching && <span className="refreshing">↻</span>}
        </div>
      </div>

      <div className="temperature-display">
        <div className="current-temp">
          <span className="temp-value">
            {data.temperature !== null ? data.temperature.toFixed(1) : "--"}
          </span>
          <span className="temp-unit">°C</span>
        </div>

        {data.sensorFailures !== null && data.sensorFailures > 0 && (
          <div className="sensor-warning">
            ⚠️ Sensor failures: {data.sensorFailures}
          </div>
        )}
      </div>

      <div className="additional-info">
        <div className="info-item">
          <span className="info-label">Humidity</span>
          <span className="info-value">
            {data.humidity !== null ? `${data.humidity.toFixed(0)}%` : "--"}
          </span>
        </div>

        <div className="info-item">
          <span className="info-label">Setpoint</span>
          <span className="info-value">
            {data.currentSetpoint.toFixed(1)}°C
            {hasOverride && <span className="override-badge">Manual</span>}
          </span>
        </div>

        {data.predictedTemperature !== null && (
          <div className="info-item">
            <span className="info-label">Predicted</span>
            <span className="info-value">
              {data.predictedTemperature.toFixed(1)}°C
            </span>
          </div>
        )}
      </div>

      {hasOverride && (
        <div className="override-info">
          {data.overrideWithoutExpiry ? (
            <p>Manual override active (until cancelled)</p>
          ) : (
            <p>{getOverrideExpiryLabel(data.overrideEnd)}</p>
          )}
        </div>
      )}
    </div>
  );
}
