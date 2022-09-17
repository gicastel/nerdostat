using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace Nerdostat.Device
{
    public class OuputPin
    {
        private bool On;
        private readonly int Pin;

        public OuputPin(int pinNumber)
        {
            this.Pin = pinNumber;
        }

        public void TurnOn()
        {
            using var Controller = new GpioController();
            Controller.OpenPin(Pin);
            Controller.SetPinMode(Pin, PinMode.Output);
            
            Controller.Write(Pin, PinValue.High);
            On = true;

            Console.WriteLine($"INFO: Led {Pin} On");
        }

        public void TurnOff()
        {
            using var Controller = new GpioController();
            Controller.OpenPin(Pin);
            Controller.SetPinMode(Pin, PinMode.Output);

            Controller.Write(Pin, PinValue.Low);
            On = false;

            Console.WriteLine($"INFO: Led {Pin} Off");
        }

        public async Task Blink(decimal OnDuration, decimal OffDuration, CancellationToken cts)
        {
            using var Controller = new GpioController();
            Controller.OpenPin(Pin);
            Controller.SetPinMode(Pin, PinMode.Output);

            while (!cts.IsCancellationRequested)
            {
                Controller.Write(Pin, PinValue.High);
                await Task.Delay(Convert.ToInt32(OnDuration * 1000), CancellationToken.None);
                Controller.Write(Pin, PinValue.Low);
                await Task.Delay(Convert.ToInt32(OffDuration * 1000), CancellationToken.None);
            }
            Controller.Write(Pin, PinValue.Low);

            On = false;
        }

        public bool IsOn() => On;
    }

}