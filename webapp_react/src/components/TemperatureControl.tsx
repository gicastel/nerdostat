import { useState } from "react";
import {
  useSetManualSetpoint,
  useClearSetpoint,
} from "../hooks/useDeviceState";
import { DURATION_OPTIONS } from "../types/models";
import "./TemperatureControl.css";

export function TemperatureControl() {
  const [temperature, setTemperature] = useState(20);
  const [durationMinutes, setDurationMinutes] = useState<number | null>(60);

  const setpointMutation = useSetManualSetpoint();
  const clearMutation = useClearSetpoint();

  const handleSetTemperature = () => {
    setpointMutation.mutate({
      temperature,
      durationMinutes,
      overrideWithoutExpiry: durationMinutes === null,
    });
  };

  const handleClear = () => {
    clearMutation.mutate();
  };

  const isPermanent = durationMinutes === null;

  return (
    <div className="temperature-control">
      <h3>Manual Temperature Control</h3>

      <div className="temp-input-section">
        <label htmlFor="temp-slider">Target Temperature</label>
        <div className="temp-slider-container">
          <div className="temp-slider-wrapper">
            <input
              id="temp-slider"
              type="range"
              min="5"
              max="28"
              step="0.5"
              value={temperature}
              onChange={(e) => {
                const rawValue = parseFloat(e.target.value);
                const snappedValue = Math.round(rawValue * 2) / 2;
                setTemperature(snappedValue);
              }}
              className="temp-slider"
            />
            <div className="temp-ticks" aria-hidden="true">
              <span style={{ left: "0%" }} />
              <span style={{ left: "21.739%" }} />
              <span style={{ left: "43.478%" }} />
              <span style={{ left: "65.217%" }} />
              <span style={{ left: "86.956%" }} />
              <span style={{ left: "100%" }} />
            </div>
          </div>
          <div className="temp-display-large">{temperature.toFixed(1)}°C</div>
        </div>
      </div>

      <div className="duration-section">
        <label>Duration</label>
        <div className="duration-options">
          {DURATION_OPTIONS.map((option) => (
            <button
              key={option.label}
              className={`duration-btn ${
                durationMinutes === option.minutes ? "active" : ""
              }`}
              onClick={() => setDurationMinutes(option.minutes)}
            >
              {option.label}
            </button>
          ))}
        </div>
      </div>

      <div className="action-buttons">
        <button
          className="btn btn-primary"
          onClick={handleSetTemperature}
          disabled={setpointMutation.isPending}
        >
          {setpointMutation.isPending ? "Setting..." : "Set Temperature"}
        </button>

        <button
          className="btn btn-secondary"
          onClick={handleClear}
          disabled={clearMutation.isPending}
        >
          {clearMutation.isPending ? "Clearing..." : "Clear Override"}
        </button>
      </div>

      {setpointMutation.isError && (
        <div className="error-message">
          Error: {setpointMutation.error?.message}
        </div>
      )}

      {clearMutation.isError && (
        <div className="error-message">
          Error: {clearMutation.error?.message}
        </div>
      )}

      {isPermanent && (
        <div className="info-message">
          Temperature will stay at {temperature.toFixed(1)}°C until manually
          cleared
        </div>
      )}
    </div>
  );
}
