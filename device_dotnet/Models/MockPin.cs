using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nerdostat.Device.Models
{
    public class MockPin
    {
        private bool On;
        public int Pin;

        private readonly ILogger Log;
        private readonly string Name;

        public MockPin(int pinNumber, ILogger _logger, string name)
        {
            Pin = pinNumber;
            Log = _logger;
            Name = name;
        }

        public void TurnOn()
        {
            On = true;
            Log.LogInformation("{Name} ON", Name);
        }

        public void TurnOff()
        {
            On = false;
            Log.LogInformation("{Name} OFF", Name);
        }

        public async Task Blink(decimal OnDuration, decimal OffDuration, CancellationToken cts)
        {
            Log.LogInformation("{Name} blinking", Name);
            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(Convert.ToInt32((OnDuration + OffDuration) * 1000), CancellationToken.None);
            }

            Log.LogInformation("{Name} {status}", Name, On ? "ON" : "OFF");
        }

        public bool IsOn() => On;

    }
}

