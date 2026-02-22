import { BrowserRouter as Router, Routes, Route, Link } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ThermostatDisplay } from "./components/ThermostatDisplay";
import { TemperatureControl } from "./components/TemperatureControl";
import { AwayModeToggle } from "./components/AwayModeToggle";
import { ProgramEditor } from "./components/ProgramEditor";
import { AnalyticsPage } from "./pages/AnalyticsPage";
import "./App.css";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
});

function Dashboard() {
  return (
    <div className="dashboard">
      <ThermostatDisplay />
      <div className="controls-grid">
        <TemperatureControl />
        <AwayModeToggle />
      </div>
    </div>
  );
}

function Program() {
  return (
    <div className="program-page">
      <ProgramEditor />
    </div>
  );
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <Router future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
        <div className="app">
          <header className="app-header">
            <h1>🌡️ Nerdostat</h1>
            <nav className="app-nav">
              <Link to="/" className="nav-link">
                Dashboard
              </Link>
              <Link to="/program" className="nav-link">
                Program
              </Link>
              <Link to="/analytics" className="nav-link">
                Analytics
              </Link>
            </nav>
          </header>

          <main className="app-main">
            <Routes>
              <Route path="/" element={<Dashboard />} />
              <Route path="/program" element={<Program />} />
              <Route path="/analytics" element={<AnalyticsPage />} />
            </Routes>
          </main>

          <footer className="app-footer">
            <p>Nerdostat - DIY IoT Thermostat</p>
          </footer>
        </div>
      </Router>
    </QueryClientProvider>
  );
}

export default App;
