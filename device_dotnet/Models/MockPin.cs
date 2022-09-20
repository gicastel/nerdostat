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

        private readonly ILogger log;

        public MockPin(int pinNumber, ILogger _logger)
        {
            Pin = pinNumber;
            log = _logger;
        }

        public void TurnOn()
        {
            On = true;
            log.LogInformation($"Led {Pin} On");
        }

        public void TurnOff()
        {
            On = false;
            log.LogInformation($"Led {Pin} Off");
        }

        public async Task Blink(decimal OnDuration, decimal OffDuration, CancellationToken cts)
        {
            log.LogInformation($"Led {Pin} started blinking");
            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(Convert.ToInt32((OnDuration + OffDuration) * 1000), CancellationToken.None);
            }

            log.LogInformation($"Led {Pin} Off");
            On = false;
        }

        public bool IsOn() => On;

    }
}

