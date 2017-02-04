using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Gaming.Input;

namespace Initio_4tronix.CarController
{
    public class MotorCommand
    {
        /// <summary>
        /// From 0.0 to 1.0
        /// </summary>
        public double Speed { get; set; }
        /// <summary>
        /// Forward: true Backward: false
        /// </summary>
        public bool ForwardBackward { get; set; }
        /// <summary>
        /// Left: from -1.0 to 0.0 Right: from 0.0 to -1
        /// </summary>
        public double RightLeft { get; set; }
        public bool FullLeft { get; set; }
        public bool FullRight { get; set; }

        public MotorCommand() { }

        public MotorCommand(GamepadReading gamepadReading)
        {
            var deadzone = 0.25;

            var leftThumbstickX = gamepadReading.LeftThumbstickX;
            if ((leftThumbstickX > 0 && leftThumbstickX <= deadzone)
                || (leftThumbstickX < 0 && leftThumbstickX >= (deadzone * -1)))
            {
                leftThumbstickX = 0.0;
            }

            var rightTrigger = gamepadReading.RightTrigger <= deadzone ? 0.0 : gamepadReading.RightTrigger;
            var leftTrigger = gamepadReading.LeftTrigger <= deadzone ? 0.0 : gamepadReading.LeftTrigger;

            var rightLeftTrigger = (rightTrigger > 0.0) && (leftTrigger > 0.0);

            if (!rightLeftTrigger && rightTrigger > 0.0)
            {
                Speed = rightTrigger;
                ForwardBackward = true;
            }
            else if (!rightLeftTrigger && leftTrigger > 0.0)
            {
                Speed = leftTrigger;
                ForwardBackward = false;
            }
            else
            {
                Speed = 0.0;
                ForwardBackward = true;
            }

            RightLeft = leftThumbstickX;

            if (RightLeft <= -0.985)
            {
                FullLeft = true;
            }
            else if (RightLeft >= 0.985)
            {
                FullRight = true;
            }

            if (RightLeft != 0.0 && Speed == 0.0)
            {
                if (RightLeft < 0)
                {
                    FullLeft = true;
                    Speed = 1;
                }
                else if (RightLeft > 0)
                {
                    FullRight = true;
                    Speed = 1;
                }
            }
        }
    }
}