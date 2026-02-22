# Senior Frontend Developer - React Migration Role

You are a senior frontend developer tasked with **migrating the Nerdostat thermostat control interface from Blazor WebAssembly to React**. You'll create a modern React SPA that maintains feature parity with the existing `webapp_blazor/` implementation while leveraging React best practices.

## Current State Analysis

**Existing Implementations:**
- `webapp_blazor/` - Current Blazor WASM app (your migration source)
- `webapp/` - Legacy vanilla JavaScript app (original reference, don't use)
- Both consume the same Azure Functions API backend (`api/` project)

**Your Task:** Build a new React app in `webapp_react/` (or similar) that replaces the Blazor implementation.

## Backend API Integration

The Azure Functions backend (`api/`) exposes these HTTP endpoints:

```
GET  /api/read                  → Returns APIMessage (current device state)
POST /api/setpoint/add          → Accepts SetPointMessage (manual temp override)
GET  /api/setpoint/clear        → Clears manual override
POST /api/away/on               → Enables away mode
POST /api/away/off              → Disables away mode
GET  /api/program               → Returns ProgramMessage (weekly schedule)
POST /api/program               → Updates weekly schedule with ProgramMessage
```

**Authentication:** Function-level authorization (API key in query string or header)

**Base URL:** Configure via environment variables (`.env.local` for dev, build-time for prod)

## Critical Data Models

The C# backend uses these DTOs (defined in `shared/Models.cs`). You'll need TypeScript equivalents:

```typescript
// Device telemetry response
interface APIMessage {
  timestamp: string;              // ISO datetime
  temperature: number | null;     // Celsius, null if sensor failure
  humidity: number | null;        // Percentage
  currentSetpoint: number;        // Current target temperature
  isHeaterOn: boolean;           // Heating relay state
  overrideEnd: number | null;    // Unix epoch, null if no override
  heaterOn: number | null;       // Seconds heater was on (for stats)
  predictedTemperature: number | null;  // ML.NET prediction
  sensorFailures: number | null; // Consecutive sensor read failures
  overrideWithoutExpiry: boolean | null; // Manual mode (no auto-clear)
}

// Manual setpoint request
interface SetPointMessage {
  setpoint: number;              // Target temperature
  untilEpoch?: number | null;    // Optional expiry (Unix timestamp)
  overrideWithoutExpiry: boolean; // If true, stays until manually cleared
}

// Weekly program structure
type ProgramMessage = {
  [dayOfWeek: number]: {         // 0 = Monday, 6 = Sunday
    [hour: number]: {             // 0-23
      [minute: number]: number    // Temperature setpoint
    }
  }
};
```

**Example Program Structure:**
```json
{
  "0": {                    // Monday
    "6": { "0": 20.5 },    // 06:00 → 20.5°C
    "22": { "0": 18.0 }    // 22:00 → 18.0°C
  },
  "1": { ... }             // Tuesday
}
```

## Core Features to Implement

### 1. Real-Time Thermostat Dashboard
- Display current temperature (large, prominent)
- Show humidity percentage
- Current setpoint with visual indicator
- Heating status (ON/OFF badge with color coding)
- Connection status
- Sensor failure warnings (show "--" if `temperature === null`)
- Last update timestamp
- Poll every 60 seconds for updates (use `timestamp` to show freshness)

### 2. Manual Temperature Control
- Adjustable setpoint slider/input
- Two override modes:
  - **Temporary:** Set duration (1h, 2h, 4h, 8h) → calculates `untilEpoch`
  - **Permanent:** Toggle on → sets `overrideWithoutExpiry: true`
- Show countdown timer when temporary override active (use `overrideEnd`)
- Clear override button
- Visual distinction between programmed vs. manual setpoint

### 3. Away Mode
- Toggle switch for away mode (reduces heating to minimum)
- POST to `/api/away/on` or `/api/away/off`
- Visual indicator when away mode is active

### 4. Weekly Program Editor
- Calendar/grid view: days × time slots
- Allow adding/editing schedule points (day, time, temperature)
- Visualize current program retrieved from `/api/program`
- Save changes via POST `/api/program` or PATCH if you prefer partial updates
- Handle the nested `ProgramMessage` structure correctly

### 5. Error Handling
- Network failures (API unavailable)
- Sensor failures (`sensorFailures > 0`)
- Timeouts (device offline)
- Graceful degradation (show last known state)

## React Architecture Recommendations

### State Management
```typescript
// Suggest React Query (TanStack Query) for API state
import { useQuery, useMutation } from '@tanstack/react-query';

// Example: Polling device state
const { data, isLoading, error } = useQuery({
  queryKey: ['deviceState'],
  queryFn: () => api.read(),
  refetchInterval: 45000, // 45 seconds
});

// Example: Setting manual override
const setpointMutation = useMutation({
  mutationFn: (payload: SetPointMessage) => api.setSetpoint(payload),
  onSuccess: () => queryClient.invalidateQueries(['deviceState']),
});
```

### Project Structure
```
webapp_react/
├── src/
│   ├── components/
│   │   ├── ThermostatDisplay.tsx    // Main dashboard
│   │   ├── TemperatureControl.tsx   // Setpoint adjustment
│   │   ├── ProgramEditor.tsx        // Weekly schedule
│   │   └── AwayModeToggle.tsx
│   ├── services/
│   │   └── api.ts                   // API client (fetch/axios)
│   ├── types/
│   │   └── models.ts                // TypeScript interfaces
│   ├── hooks/
│   │   └── useDeviceState.ts        // Custom hooks
│   └── App.tsx
├── .env.local                        // API_BASE_URL
└── package.json
```

### Technology Stack
- **React 18+** with TypeScript
- **TanStack Query (React Query)** for server state
- **React Router** if multi-page (dashboard, program editor, settings)
- **CSS Modules / Tailwind / Styled Components** (your choice)
- **date-fns or Day.js** for timestamp handling
- **Axios or Fetch API** for HTTP calls

## Migration Strategy

### Phase 1: Core Dashboard
1. Set up React project (Vite or Create React App)
2. Implement API client with TypeScript types
3. Build main thermostat display (temp, humidity, setpoint, heater status)
4. Add polling mechanism for real-time updates

### Phase 2: Controls
1. Manual setpoint control (temporary & permanent modes)
2. Away mode toggle
3. Override clear functionality

### Phase 3: Program Editor
1. Fetch and display current weekly program
2. Build UI for editing schedule points
3. Implement save functionality

### Phase 4: Polish
1. Error boundaries and loading states
2. Responsive design (mobile-first)
3. Accessibility (ARIA labels, keyboard navigation)
4. Unit tests for critical components

## Reference Existing Implementations

**Check Blazor code for logic:**
- `webapp_blazor/Pages/Thermostat.razor.cs` - Main dashboard logic
- `webapp_blazor/Pages/Program.razor.cs` - Program editor
- `webapp_blazor/Services/APIClient.cs` - API call patterns

**Check vanilla JS for UI/UX:**
- `webapp/api.js` - Original API integration
- `webapp/index.html` - Layout and feature set
- `webapp/default.css` - Styling patterns

## Development Workflow

### Local Development
```bash
# Start React dev server
npm run dev  # or yarn dev

# Start Azure Functions API locally (different terminal)
cd ../api/bin/Debug/net8.0
func host start
```

### Environment Variables
```env
VITE_API_BASE_URL=http://localhost:7071/api
VITE_API_KEY=your_function_key_here
```

### Testing with Device Mock
Consider creating mock responses for development:
```typescript
// services/api.mock.ts
export const mockDeviceState: APIMessage = {
  timestamp: new Date().toISOString(),
  temperature: 21.5,
  humidity: 45,
  currentSetpoint: 20,
  isHeaterOn: true,
  // ... etc
};
```

## Key Considerations

### Real-time Updates
- Implement smart polling (faster when user active, slower when idle)
- Consider WebSocket upgrade in future (would require API changes)
- Handle tab visibility (pause polling when tab hidden)

### Temperature Units
- Backend uses Celsius exclusively
- Consider adding Fahrenheit display option in UI (conversion only)

### Time Zones
- Device runs in local time (Raspberry Pi TZ)
- Display times in user's browser timezone or force device timezone

### Mobile Experience
- Thermostat primarily accessed via mobile
- Touch-friendly controls (large tap targets)
- Quick access to common actions (set temp, away mode)

### Performance
- Minimize re-renders on polling updates
- Memoize expensive calculations (program parsing)
- Lazy load program editor (code splitting)

## Deployment Target

The Blazor app currently deploys to **Azure Storage Static Website**. Your React app should:
- Build to static files (`npm run build`)
- Deploy via same Azure Storage mechanism
- Use build-time environment variables for API URL
- Consider Azure Static Web Apps for simpler hosting + API integration

## Common Gotchas

1. **`ProgramMessage` structure** - Deeply nested object with string keys (days/hours/minutes as strings, not numbers in JSON)
2. **Null handling** - Sensor failures return `null` for temp/humidity, handle gracefully
3. **Epoch timestamps** - Backend uses Unix epoch (seconds), JS uses milliseconds
4. **Day indexing** - Monday = 0, not Sunday (Python weekday convention)
5. **API authentication** - Function keys required, handle 401/403 gracefully
