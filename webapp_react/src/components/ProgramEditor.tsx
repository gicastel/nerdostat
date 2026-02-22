import { useState, useEffect } from 'react';
import { useProgram, useUpdateProgram } from '../hooks/useDeviceState';
import { ProgramMessage, ProgramEntry, DAY_NAMES } from '../types/models';
import './ProgramEditor.css';

export function ProgramEditor() {
  const { data: program, isLoading, error } = useProgram();
  const updateMutation = useUpdateProgram();
  
  const [entries, setEntries] = useState<ProgramEntry[]>([]);
  const [editingEntry, setEditingEntry] = useState<ProgramEntry | null>(null);

  useEffect(() => {
    if (program) {
      const parsed = parseProgramMessage(program);
      setEntries(parsed);
    }
  }, [program]);

  const parseProgramMessage = (program: ProgramMessage): ProgramEntry[] => {
    const result: ProgramEntry[] = [];
    
    for (const [dayStr, hours] of Object.entries(program)) {
      const day = parseInt(dayStr);
      for (const [hourStr, minutes] of Object.entries(hours)) {
        const hour = parseInt(hourStr);
        for (const [minuteStr, temperature] of Object.entries(minutes)) {
          const minute = parseInt(minuteStr);
          result.push({ day, hour, minute, temperature });
        }
      }
    }
    
    return result.sort((a, b) => 
      a.day !== b.day ? a.day - b.day : 
      a.hour !== b.hour ? a.hour - b.hour : 
      a.minute - b.minute
    );
  };

  const buildProgramMessage = (entries: ProgramEntry[]): ProgramMessage => {
    const program: ProgramMessage = {};
    
    for (const entry of entries) {
      const dayKey = entry.day.toString();
      const hourKey = entry.hour.toString();
      const minuteKey = entry.minute.toString();
      
      if (!program[dayKey]) program[dayKey] = {};
      if (!program[dayKey][hourKey]) program[dayKey][hourKey] = {};
      program[dayKey][hourKey][minuteKey] = entry.temperature;
    }
    
    return program;
  };

  const handleAddEntry = () => {
    setEditingEntry({
      day: 0,
      hour: 8,
      minute: 0,
      temperature: 20,
    });
  };

  const handleSaveEntry = (entry: ProgramEntry) => {
    // Remove existing entry at same time if any
    const filtered = entries.filter(
      e => !(e.day === entry.day && e.hour === entry.hour && e.minute === entry.minute)
    );
    const newEntries = [...filtered, entry];
    setEntries(newEntries);
    setEditingEntry(null);
  };

  const handleDeleteEntry = (entry: ProgramEntry) => {
    const filtered = entries.filter(
      e => !(e.day === entry.day && e.hour === entry.hour && e.minute === entry.minute)
    );
    setEntries(filtered);
  };

  const handleSaveProgram = () => {
    const programMessage = buildProgramMessage(entries);
    updateMutation.mutate(programMessage);
  };

  if (isLoading) {
    return <div className="program-editor loading">Loading program...</div>;
  }

  if (error) {
    return (
      <div className="program-editor error">
        <p>Error loading program: {error.message}</p>
      </div>
    );
  }

  return (
    <div className="program-editor">
      <div className="program-header">
        <h3>Weekly Heating Schedule</h3>
        <button className="btn btn-add" onClick={handleAddEntry}>
          + Add Entry
        </button>
      </div>

      <div className="program-entries">
        {entries.map((entry, index) => (
          <div key={index} className="program-entry">
            <div className="entry-time">
              <span className="entry-day">{DAY_NAMES[entry.day]}</span>
              <span className="entry-clock">
                {String(entry.hour).padStart(2, '0')}:{String(entry.minute).padStart(2, '0')}
              </span>
            </div>
            <div className="entry-temp">{entry.temperature.toFixed(1)}°C</div>
            <button
              className="btn-delete"
              onClick={() => handleDeleteEntry(entry)}
              aria-label="Delete entry"
            >
              ✕
            </button>
          </div>
        ))}

        {entries.length === 0 && (
          <div className="empty-state">
            No schedule entries. Click "Add Entry" to create one.
          </div>
        )}
      </div>

      {editingEntry && (
        <EntryForm
          entry={editingEntry}
          onSave={handleSaveEntry}
          onCancel={() => setEditingEntry(null)}
        />
      )}

      <div className="program-actions">
        <button
          className="btn btn-primary"
          onClick={handleSaveProgram}
          disabled={updateMutation.isPending}
        >
          {updateMutation.isPending ? 'Saving...' : 'Save Program'}
        </button>
      </div>

      {updateMutation.isError && (
        <div className="error-message">
          Error: {updateMutation.error?.message}
        </div>
      )}

      {updateMutation.isSuccess && (
        <div className="success-message">
          Program updated successfully!
        </div>
      )}
    </div>
  );
}

interface EntryFormProps {
  entry: ProgramEntry;
  onSave: (entry: ProgramEntry) => void;
  onCancel: () => void;
}

function EntryForm({ entry, onSave, onCancel }: EntryFormProps) {
  const [formData, setFormData] = useState(entry);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSave(formData);
  };

  return (
    <div className="entry-form-overlay">
      <form className="entry-form" onSubmit={handleSubmit}>
        <h4>Add Schedule Entry</h4>

        <div className="form-group">
          <label htmlFor="day">Day</label>
          <select
            id="day"
            value={formData.day}
            onChange={(e) => setFormData({ ...formData, day: parseInt(e.target.value) })}
          >
            {DAY_NAMES.map((name, index) => (
              <option key={index} value={index}>
                {name}
              </option>
            ))}
          </select>
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="hour">Hour</label>
            <input
              id="hour"
              type="number"
              min="0"
              max="23"
              value={formData.hour}
              onChange={(e) => setFormData({ ...formData, hour: parseInt(e.target.value) })}
            />
          </div>

          <div className="form-group">
            <label htmlFor="minute">Minute</label>
            <input
              id="minute"
              type="number"
              min="0"
              max="59"
              step="15"
              value={formData.minute}
              onChange={(e) => setFormData({ ...formData, minute: parseInt(e.target.value) })}
            />
          </div>
        </div>

        <div className="form-group">
          <label htmlFor="temperature">Temperature (°C)</label>
          <input
            id="temperature"
            type="number"
            min="15"
            max="25"
            step="0.5"
            value={formData.temperature}
            onChange={(e) => setFormData({ ...formData, temperature: parseFloat(e.target.value) })}
          />
        </div>

        <div className="form-actions">
          <button type="submit" className="btn btn-primary">
            Save
          </button>
          <button type="button" className="btn btn-secondary" onClick={onCancel}>
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
}
