using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using System;
using BlazorClient.Services;
using Nerdostat.Shared;

namespace BlazorClient.Pages
{
    public class ThermostatBase : ComponentBase
    {
        [Inject] IAPIClient _client { get; set; }

        protected APIMessage status { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                status = await _client.GetData();
            }
            catch (Exception ex)
            {
                // Pokemon Exception Handler (TM)
            }
        }

        protected async Task TempUp()
        {
            double newTemp = status.CurrentSetpoint + 0.5;
            status = await _client.ModifySetPoint(newTemp, null);
        }

        protected async Task TempDown()
        {
            double newTemp = status.CurrentSetpoint - 0.5;
            status = await _client.ModifySetPoint(newTemp, null);
        }

        protected async Task Reset()
        {
            status = await _client.ResetSetPoint();
        }
    }
}
