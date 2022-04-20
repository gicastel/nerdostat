using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.DHTxx;

namespace Nerdostat.Device
{
    public class OuputPin
    {
        private bool On;
        private int Pin;

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
        }

        public void TurnOff()
        {
            using var Controller = new GpioController();
            Controller.OpenPin(Pin);
            Controller.SetPinMode(Pin, PinMode.Output);

            Controller.Write(Pin, PinValue.Low);
            On = false;
        }

        public async Task Blink(decimal OnDuration, decimal OffDuration, CancellationToken cts)
        {
            using var Controller = new GpioController();
            Controller.OpenPin(Pin);
            Controller.SetPinMode(Pin, PinMode.Output);

            while (!cts.IsCancellationRequested)
            {
                Controller.Write(Pin, PinValue.High);
                await Task.Delay(Convert.ToInt32(OnDuration * 1000));
                Controller.Write(Pin, PinValue.Low);
                await Task.Delay(Convert.ToInt32(OffDuration * 1000));
            }
            Controller.Write(Pin, PinValue.Low);

            On = false;
        }

        public bool IsOn() => On;
    }

}