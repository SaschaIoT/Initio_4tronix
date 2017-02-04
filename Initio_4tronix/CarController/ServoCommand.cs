using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Gaming.Input;

namespace Initio_4tronix.CarController
{
    public class ServoCommand
    {
        public bool Up { get; set; }
        public bool Down { get; set; }
        public ushort Speed { get; set; }
        
        public ServoCommand() { }

        public ServoCommand(GamepadReading gamepadReading)
        {
            var speed = 6;
            var deadzone = 0.25;

            var leftThumbstickY = gamepadReading.LeftThumbstickY;
            if ((leftThumbstickY > 0 && leftThumbstickY <= deadzone)
                || (leftThumbstickY < 0 && leftThumbstickY >= (deadzone * -1)))
            {
                leftThumbstickY = 0.0;
            }

            var thumbstickY = leftThumbstickY;

            if (thumbstickY > 0)
            {
                Up = true;
            }
            else if (thumbstickY < 0)
            {
                Down = true;
            }

            Speed = (ushort)Math.Round(Math.Abs(thumbstickY) * speed, 1);
        }
    }
}
