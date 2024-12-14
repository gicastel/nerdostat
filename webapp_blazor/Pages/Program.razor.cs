using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using System;
using BlazorClient.Services;
using Nerdostat.Shared;
using Microsoft.JSInterop;

namespace BlazorClient.Pages
{
    public class ProgramBase : ComponentBase
    {
        [Inject] IAPIClient _client { get; set; }

        [Inject] IJSRuntime JSRuntime { get; set; }
        protected ProgramMessage status { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                status = await _client.GetProgram();
            }
            catch (Exception ex)
            {
                // Pokemon Exception Handler (TM)
                status = new ProgramMessage();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("renderProgramChart", status);
        }
    }

        protected async Task Save()
        {
            status = await _client.UpdateProgram(status);
        }

        protected async Task Reset()
        {
            status = await _client.GetProgram();
        }
    }
}
