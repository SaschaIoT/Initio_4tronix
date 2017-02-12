using System;

namespace Initio_4tronix.CarController
{
    public class Motor
    {
        public MotorController MotorController { get; set; }
        public int MotorNumber { get; set; }
        public int MotorPwmPin { get; set; }
        public int MotorIn1Pin { get; set; }
        public int MotorIn2Pin { get; set; }

        public Motor(MotorController motorController, int motorNumber)
        {
            MotorController = motorController;
            MotorNumber = motorNumber;
            int motorPwmPin = 0, motorIn2Pin = 0, motorIn1Pin = 0;

            if (motorNumber == 0)
            {
                motorPwmPin = 8;
                motorIn1Pin = 9;
                motorIn2Pin = 10;
            }
            else if (motorNumber == 1)
            {
                motorPwmPin = 13;
                motorIn1Pin = 12;
                motorIn2Pin = 11;
            }
            else if (motorNumber == 2)
            {
                motorPwmPin = 2;
                motorIn1Pin = 3;
                motorIn2Pin = 4;
            }
            else if (motorNumber == 3)
            {
                motorPwmPin = 7;
                motorIn1Pin = 6;
                motorIn2Pin = 5;
            }
            else
            {
                throw new Exception("Motor must be between 1 and 4 inclusive");
            }

            MotorPwmPin = motorPwmPin;
            MotorIn1Pin = motorIn2Pin;
            MotorIn2Pin = motorIn1Pin;
        }

        public void Run(MotorAction command)
        {
            if (MotorController == null)
            {
                return;
            }

            if (command == MotorAction.FORWARD)
            {
                MotorController.SetPin(MotorIn2Pin, 0);
                MotorController.SetPin(MotorIn1Pin, 1);
            }
            else if (command == MotorAction.BACKWARD)
            {
                MotorController.SetPin(MotorIn1Pin, 0);
                MotorController.SetPin(MotorIn2Pin, 1);
            }
            else if (command == MotorAction.RELEASE)
            {
                MotorController.SetPin(MotorIn1Pin, 0);
                MotorController.SetPin(MotorIn2Pin, 0);
            }
        }

        public void SetSpeed(int speed)
        {
            if (speed < 0)
            {
                speed = 0;
            }
            else if (speed > 255)
            {
                speed = 255;
            }

            MotorController.PwmController.SetPwm((byte)MotorPwmPin, 0, (ushort)(speed * 16));
        }
    }

    public enum MotorAction
    {
        FORWARD = 1,
        BACKWARD = 2,
        BRAKE = 3,
        RELEASE = 4,
        SINGLE = 1,
        DOUBLE = 2,
        INTERLEAVE = 3,
        MICROSTEP = 4
    }
}
