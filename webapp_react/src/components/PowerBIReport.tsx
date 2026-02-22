import './PowerBIReport.css';

interface PowerBIReportProps {
  reportUrl?: string;
  embedded?: boolean;
}

export function PowerBIReport({ reportUrl, embedded = false }: PowerBIReportProps) {
  // Default Power BI workspace URL - customize with your actual report
  const defaultReportUrl = 'https://app.powerbi.com/groups/me/reports';
  const url = reportUrl || defaultReportUrl;

  if (embedded && url && url.includes('app.powerbi.com')) {
    // Extract report ID from Power BI URL if available
    return (
      <div className="powerbi-embedded">
        <iframe
          title="Nerdostat Report"
          src={url}
          className="powerbi-iframe"
          allowFullScreen
        />
        <p className="powerbi-disclaimer">
          Power BI report. Some features may require authentication.
        </p>
      </div>
    );
  }

  return (
    <div className="powerbi-report">
      <h3>📊 Analytics Dashboard</h3>
      <p className="report-description">
        View detailed analytics and historical data in Power BI
      </p>
      <a
        href={url}
        target="_blank"
        rel="noopener noreferrer"
        className="btn btn-powerbi"
      >
        Open Power BI Report →
      </a>
      <p className="report-note">
        Opens in a new window. Requires Power BI account access.
      </p>
    </div>
  );
}
