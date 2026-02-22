import { PowerBIReport } from '../components/PowerBIReport';
import './AnalyticsPage.css';

export function AnalyticsPage() {
  return (
    <div className="analytics-page">
      <h2>📊 Analytics & Reports</h2>
      <div className="analytics-container">
        <PowerBIReport reportUrl={import.meta.env.VITE_POWERBI_REPORT_URL} embedded={true} />
      </div>
    </div>
  );
}
