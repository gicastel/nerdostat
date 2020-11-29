using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using System;
using BlazorClient.Services;
using Nerdostat.Shared;
using BlazorClient.Models;

namespace BlazorClient.Pages
{
    public class ThermostatBase : ComponentBase
    {
        [Inject] IAPIClient _client { get; set; }

        protected APIMessage status { get; set; }

        protected long? OverrideEndInMinutes { get; set; }
        protected string OverrideUntil => (OverrideEndInMinutes.HasValue ? DateTime.Now.AddMinutes(Convert.ToDouble(OverrideEndInMinutes)).ToString("HH:mm dd/MM") : "--" );

        protected string ConnectionIcon = ConnectionStatusIcon.OFF;

        protected string HeaterIcon = HeaterStatusIcon.OFF;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                status = await _client.GetData();
                ConnectionIcon = ConnectionStatusIcon.ON;
                RefreshStatus();
            }
            catch (Exception ex)
            {
                // Pokemon Exception Handler (TM)
                ConnectionIcon = ConnectionStatusIcon.OFF;
            }
        }

        protected void RefreshStatus()
        {
            if (status.OverrideEnd.HasValue)
                this.OverrideEndInMinutes = status.OverrideEnd / 60000;
            else
                this.OverrideEndInMinutes = 0;

            HeaterIcon = status.IsHeaterOn ? HeaterStatusIcon.ON : HeaterStatusIcon.OFF;
        }


        protected async Task TempUp()
        {
            double newTemp = status.CurrentSetpoint + 0.5;
            status = await _client.ModifySetPoint(newTemp, OverrideEndInMinutes / 60);
            RefreshStatus();
        }

        protected async Task TempDown()
        {
            double newTemp = status.CurrentSetpoint - 0.5;
            status = await _client.ModifySetPoint(newTemp, OverrideEndInMinutes / 60);
            RefreshStatus();

        }

        protected async Task ChangeSetpointDuration()
        {
            status = await _client.ModifySetPoint(status.CurrentSetpoint, OverrideEndInMinutes / 60);
            RefreshStatus();
        }

        protected async Task Reset()
        {
            status = await _client.ResetSetPoint();
            RefreshStatus();
        }
    }
}
