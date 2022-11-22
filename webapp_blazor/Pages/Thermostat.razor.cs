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
            try
            {
                status = await _client.GetData();
                ConnectionIcon = ConnectionStatusIcon.ON;
            }
            catch 
            {
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
            finally
            {
                RefreshStatus();
            }
        }

        protected void RefreshStatus()
        {
            // overrideEnd = epoch seconds
            if (status.OverrideEnd.HasValue)
            {
                DateTime setpointExpiration = status.OverrideEnd.Value.ToDateTime();
                this.OverrideEndInMinutes = (int)(setpointExpiration - DateTime.Now).TotalMinutes;
            }
            else
                this.OverrideEndInMinutes = 0;

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

            status = await _client.ModifySetPoint(newTemp, OverrideEndInMinutes);
            RefreshStatus();
        }


        protected async Task ChangeSetpointDuration()
        {
            status = await _client.ModifySetPoint(status.CurrentSetpoint, OverrideEndInMinutes);
            RefreshStatus();
        }

        protected async Task Reset()
        {
            status = await _client.ResetSetPoint();
            RefreshStatus();
        }
    }
}
