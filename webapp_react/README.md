# Nerdostat React App

Modern React implementation of the Nerdostat thermostat control interface.

## Features

- 🌡️ Real-time temperature monitoring
- 🎛️ Manual temperature control with duration presets
- 🏠 Away mode toggle
- 📅 Weekly heating program editor
- 📱 Responsive mobile-first design
- ⚡ Fast refresh with Vite
- 🔄 Optimistic UI updates with React Query

## Getting Started

### Prerequisites

- Node.js 18+ and npm (or yarn/pnpm)
- Azure Functions API running locally or in Azure

### Installation

1. Install dependencies:
```bash
npm install
```

2. Create environment configuration:
```bash
cp .env.local.example .env.local
```

3. Edit `.env.local` with your API settings:
```env
VITE_API_BASE_URL=http://localhost:7071
VITE_API_KEY=your_function_key_here
```

### Development

Start the development server:
```bash
npm run dev
```

The app will be available at `http://localhost:3000`

### Building for Production

```bash
npm run build
```

The production build will be in the `dist/` directory.

Preview the production build:
```bash
npm run preview
```

## Project Structure

```
src/
├── components/          # React components
│   ├── ThermostatDisplay.tsx
│   ├── TemperatureControl.tsx
│   ├── AwayModeToggle.tsx
│   └── ProgramEditor.tsx
├── hooks/              # Custom React hooks
│   └── useDeviceState.ts
├── services/           # API client
│   └── api.ts
├── types/             # TypeScript definitions
│   └── models.ts
├── App.tsx            # Main app component
└── main.tsx           # Entry point
```

## API Integration

The app communicates with the Azure Functions backend at the endpoints defined in `src/services/api.ts`:

- `GET /api/read` - Get device state
- `POST /api/setpoint/add` - Set manual temperature
- `POST /api/setpoint/clear` - Clear override
- `POST /api/away/on` - Enable away mode
- `POST /api/away/off` - Disable away mode
- `GET /api/program` - Get heating schedule
- `POST /api/program` - Update schedule

## Technology Stack

- **React 18** - UI framework
- **TypeScript** - Type safety
- **Vite** - Build tool and dev server
- **TanStack Query** - Server state management
- **React Router** - Client-side routing
- **date-fns** - Date formatting

## Development Workflow

### Running with Local API

1. Start the Azure Functions API:
```bash
cd ../api/bin/Debug/net8.0
func host start
```

2. In another terminal, start the React app:
```bash
npm run dev
```

### Testing

The app includes error handling for:
- Network failures
- API timeouts
- Sensor failures
- Invalid responses

Test these scenarios by stopping the API or modifying responses.

## Deployment

### Azure Storage Static Website

1. Build the production bundle:
```bash
npm run build
```

2. Upload the `dist/` folder contents to Azure Storage Static Website

3. Set production environment variables in your deployment configuration

### Azure Static Web Apps (Recommended)

Azure Static Web Apps can host the React app and integrate with the Functions API:

```bash
# Install Azure Static Web Apps CLI
npm install -g @azure/static-web-apps-cli

# Deploy
swa deploy
```

## Contributing

This is a personal DIY project. See the main repository README for architecture details.

## License

MIT
