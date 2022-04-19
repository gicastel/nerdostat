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

        private GpioController Controller;

        public OuputPin(int pinNumber)
        {
            if (Controller is null)
                Controller = new GpioController();

            this.Pin = pinNumber;
        }

        public void TurnOn()
        {
            Controller.OpenPin(Pin);
            Controller.SetPinMode(Pin, PinMode.Output);
            
            Controller.Write(Pin, PinValue.High);
            Controller.ClosePin(Pin);
            On = true;
        }

        public void TurnOff()
        {
            Controller.OpenPin(Pin);
            Controller.SetPinMode(Pin, PinMode.Output);

            Controller.Write(Pin, PinValue.Low);
            Controller.ClosePin(Pin);
            On = false;
        }

        public async Task Blink(decimal OnDuration, decimal OffDuration, CancellationToken cts)
        {
            Controller.OpenPin(Pin);
            Controller.SetPinMode(Pin, PinMode.Output);

            while (!cts.IsCancellationRequested)
            {
                Controller.Write(Pin, PinValue.High);
                await Task.Delay(Convert.ToInt32(OnDuration * 1000));
                Controller.Write(Pin, PinValue.Low);
                await Task.Delay(Convert.ToInt32(OffDuration * 1000));
            }
            Controller.ClosePin(Pin);
            On = false;
        }

        public bool IsOn() => On;
    }

}