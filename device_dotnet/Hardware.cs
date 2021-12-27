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
            using var controller = new GpioController();
            controller.OpenPin(Pin);
            controller.SetPinMode(Pin, PinMode.Output);
            
            controller.Write(Pin, PinValue.High);
            On = true;
        }

        public void TurnOff()
        {

            using var controller = new GpioController();
            controller.OpenPin(Pin);
            controller.SetPinMode(Pin, PinMode.Output);

            controller.Write(Pin, PinValue.Low);
            On = false;
        }

        public async Task Blink(decimal OnDuration, decimal OffDuration, CancellationToken cts)
        {
            using var controller = new GpioController();
            controller.OpenPin(Pin);
            controller.SetPinMode(Pin, PinMode.Output);

            while (!cts.IsCancellationRequested)
            {
                controller.Write(Pin, PinValue.High);
                await Task.Delay(Convert.ToInt32(OnDuration * 1000));
                controller.Write(Pin, PinValue.Low);
                await Task.Delay(Convert.ToInt32(OffDuration * 1000));
            }
            On = false;
        }

        public bool IsOn() => On;
    }

}