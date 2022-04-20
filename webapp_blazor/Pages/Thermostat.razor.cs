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

        protected int OverrideEndInMinutes { get; set; }
        protected string OverrideUntil => (OverrideEndInMinutes > 0 ? DateTime.Now.AddMinutes(Convert.ToDouble(OverrideEndInMinutes)).ToString("HH:mm dd/MM") : "--" );

        protected string ConnectionIcon = ConnectionStatusIcon.OFF;

        protected string HeaterIcon = HeaterStatusIcon.OFF;

        protected override async Task OnInitializedAsync()
        {
            var refresh = _client.GetData();
            var timeout = Task.Delay(10*1000);
            var first = await Task.WhenAny(refresh, timeout);
            if (first == refresh)
            {
                status = refresh.Result;
                ConnectionIcon = ConnectionStatusIcon.ON;
                RefreshStatus();
            }
            else
            {
                // Pokemon Exception Handler (TM)
                status = new APIMessage()
                {
                    Timestamp = DateTime.Now,
                    Temperature = 0,
                    Humidity = 0,
                    CurrentSetpoint = 0,
                    IsHeaterOn = false,
                    OverrideEnd = null,
                    HeaterOn = null
                };
                ConnectionIcon = ConnectionStatusIcon.OFF;
            }
        }

        protected void RefreshStatus()
        {
            if (status.OverrideEnd.HasValue)
                this.OverrideEndInMinutes = Convert.ToInt32(status.OverrideEnd / 60000);

            HeaterIcon = status.IsHeaterOn ? HeaterStatusIcon.ON : HeaterStatusIcon.OFF;
        }


        protected async Task TempUp()
        {
            await ModifySetpoint(0.5);
        }

        protected async Task TempDown()
        {
            await ModifySetpoint(-0.5);
        }

        private async Task ModifySetpoint(double tempVariation)
        {
            double newTemp = status.CurrentSetpoint + tempVariation;
            double? overrideHours = null;
            if (OverrideEndInMinutes > 0)
                // round it up, since it's an int
                overrideHours = (OverrideEndInMinutes / 60) + 1;

            status = await _client.ModifySetPoint(newTemp, overrideHours);
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
