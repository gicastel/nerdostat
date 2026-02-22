# Nerdostat - AI Coding Agent Instructions

## Architecture Overview

Nerdostat is a DIY IoT thermostat system with three interconnected .NET projects plus legacy Python device code:

- **device_dotnet/** - .NET 8 console app running on Raspberry Pi, controls GPIO pins for heating relay and DHT sensor, uses Azure IoT Device SDK
- **api/** - Azure Functions v4 (.NET 8) backend with HTTP triggers and EventHub triggers
- **webapp_blazor/** - Blazor WebAssembly client (WIP replacement for vanilla JS webapp/)
- **shared/** - Shared project containing `Models.cs` and `ExtensionMethods.cs`, imported by all .NET projects

**Data Flow**: Device sends telemetry every N minutes → Azure IoT Hub → EventHub trigger (`Hub.cs`) → Application Insights metrics + optional PowerBI. Web app calls API functions → IoT Hub cloud-to-device methods → Device responds.

## Critical Patterns & Conventions

### Shared Project System
All .NET projects import `shared/Nerdostat.Shared.projitems`. Models like `IotMessage`, `APIMessage`, `SetPointMessage`, and the `DeviceMethods` static class are defined once in `shared/Models.cs` and shared across device, API, and Blazor projects. Never duplicate these definitions.

### Azure IoT Hub Communication
- **Device → Cloud**: Device sends `IotMessage` objects every interval (configurable in `config.json`)
- **Cloud → Device**: API invokes direct methods via `IoTHub.cs` service using `CloudToDeviceMethod` class. Method names defined in `DeviceMethods` static class (e.g., `ReadNow`, `SetManualSetpoint`, `ClearManualSetPoint`)
- Device registers method handlers in `HubManager.cs` using `client.SetMethodHandlerAsync()`

### Conditional Compilation for GPIO
Device code uses `#if DEBUG` to swap `OutputPin` (real GPIO) with `MockPin` (console logging) for local development without Raspberry Pi hardware. See `Thermostat.cs` lines 19-24.

### Configuration Files
- **device_dotnet/config.json** - IoT Hub connection string, intervals, thresholds, thermostat program. Git-ignored, sample in `utils/config.sample.json`
- **api/local.settings.json** - Function app settings (connection strings, device ID, PowerBI endpoint). Git-ignored
- Both use environment variables in production (systemd for device, Azure App Settings for Functions)

### Device Services Architecture (DI Container)
`Program.cs` configures Host with:
- `HostedWorker` - main loop coordinator
- `Thermostat` - GPIO control, sensor reading, setpoint logic
- `HubManager` - Azure IoT Hub client, connection management, method handlers
- `SqliteDatastore` - local telemetry persistence
- `MeteoService` - external weather API integration
- `Predictor` - ML.NET model for temperature prediction

Services are singletons. `HostedWorker.StartAsync` kicks off main loop.

## Build & Development

### Building Projects
```powershell
# Build API (from api/ directory)
dotnet build

# Build device (from device_dotnet/ directory)
dotnet build

# Build Blazor client (from webapp_blazor/ directory)
dotnet build
```

Use VS Code tasks defined in workspace: `build (functions)` (default), `watch` (Blazor), etc.

### Running Locally
- **API**: Use task "func: 7" or `cd api/bin/Debug/net8.0; func host start` (requires Azure Functions Core Tools)
- **Device**: Debug launch with mock GPIO, or publish to Raspberry Pi using `deploy.ps1` rsync script
- **Blazor**: Task "watch" runs `dotnet watch run`

#if DEBUG disables `ReadMessage` EventHub trigger in `Hub.cs` to avoid double-processing during local dev.

### Deployment
- **Device**: `deploy.ps1` builds for `linux-arm` runtime, rsyncs to Raspberry Pi at hardcoded hostname `lizzano`, deployed as systemd service (see `utils/nerdostat.service`)
- **API**: Functions deployed via zip deploy (see `serviceDependencies` in `Properties/`)
- **Webapp**: Blazor WASM published to Azure Storage static website (infra not in repo)

## Key Files & Entry Points

- **Shared models**: `shared/Models.cs` - single source of truth for DTOs
- **Device main loop**: `device_dotnet/Services/HostedWorker.cs` - refresh cycle, send telemetry
- **Device thermostat logic**: `device_dotnet/Services/Thermostat.cs` - GPIO, sensor reading, heating control
- **Device IoT Hub client**: `device_dotnet/Services/HubManager.cs` - connection management, method handlers
- **API endpoints**: `api/App.cs` - HTTP triggers for web app (read, setpoint, away mode, program)
- **API telemetry processing**: `api/Hub.cs` - EventHub trigger, logs metrics to App Insights
- **API IoT Hub service**: `api/Services/IoTHub.cs` - wrapper for cloud-to-device method invocations
- **Blazor pages**: `webapp_blazor/Pages/Thermostat.razor` and `Program.razor`

## External Dependencies

- Azure IoT Hub (device-to-cloud messages, cloud-to-device methods)
- Azure Functions with EventHub trigger binding
- Application Insights for telemetry
- Optional PowerBI streaming dataset (configured via `PowerBiEndpoint` env var)
- External weather API via `MeteoService` (configurable endpoint)

## Testing Notes

Device has virtual GPIO mode (#if DEBUG). Python device code still exists in `device/` but .NET version is current. No automated tests in repo - testing done manually with live IoT Hub and hardware.
