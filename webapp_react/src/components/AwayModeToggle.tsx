import { useState } from 'react';
import { useAwayMode } from '../hooks/useDeviceState';
import './AwayModeToggle.css';

export function AwayModeToggle() {
  const [isAway, setIsAway] = useState(false);
  const awayModeMutation = useAwayMode();

  const handleToggle = () => {
    const newState = !isAway;
    setIsAway(newState);
    awayModeMutation.mutate(newState);
  };

  return (
    <div className="away-mode-toggle">
      <div className="away-mode-header">
        <div>
          <h3>Away Mode</h3>
          <p className="away-mode-description">
            Reduces heating to minimum when you're away
          </p>
        </div>
        <button
          className={`toggle-switch ${isAway ? 'active' : ''}`}
          onClick={handleToggle}
          disabled={awayModeMutation.isPending}
          aria-label="Toggle away mode"
        >
          <span className="toggle-slider" />
        </button>
      </div>

      {awayModeMutation.isError && (
        <div className="error-message">
          Error: {awayModeMutation.error?.message}
        </div>
      )}

      {isAway && (
        <div className="away-active-message">
          🏠 Away mode is active - heating minimized
        </div>
      )}
    </div>
  );
}
