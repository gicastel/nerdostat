using Microsoft.Extensions.Logging;
using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace Nerdostat.Device.Models
{
    public class OutputPin
    {
        private bool On;
        public int Pin;

        private readonly ILogger Log;
        private readonly string Name;

        public OutputPin(int pinNumber, ILogger _logger, string name)
        {
            Pin = pinNumber;
            Log = _logger;
            Name = name;
        }

        public void TurnOn()
        {
            using var Controller = new GpioController();
            Controller.OpenPin(Pin);
            Controller.SetPinMode(Pin, PinMode.Output);
            Controller.Write(Pin, PinValue.High);

            On = true;
            Log.LogInformation("{Name} ON", Name);
        }

        public void TurnOff()
        {
            using var Controller = new GpioController();
            Controller.OpenPin(Pin);
            Controller.SetPinMode(Pin, PinMode.Output);
            Controller.Write(Pin, PinValue.Low);

            On = false;
            Log.LogInformation("{Name} OFF", Name);
        }

        public async Task Blink(decimal OnDuration, decimal OffDuration, CancellationToken cts)
        {
            using var Controller = new GpioController();
            Controller.OpenPin(Pin);
            Controller.SetPinMode(Pin, PinMode.Output);
            Log.LogInformation("{Name} blinking", Name);

            while (!cts.IsCancellationRequested)
            {
                Controller.Write(Pin, PinValue.High);
                await Task.Delay(Convert.ToInt32(OnDuration * 1000), CancellationToken.None);
                Controller.Write(Pin, PinValue.Low);
                await Task.Delay(Convert.ToInt32(OffDuration * 1000), CancellationToken.None);
            }
            Controller.Write(Pin, On? PinValue.High : PinValue.Low);
            
            Log.LogInformation("{Name} {status}}", Name, On? "ON" : "OFF");
        }

        public bool IsOn() => On;

    }
}

