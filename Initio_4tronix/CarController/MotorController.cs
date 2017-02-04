using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Initio_4tronix.CarController
{
    public class MotorController
    {
        private int _i2caddress;
        private int _frequency;
        public PwmController PwmController;
        public List<Motor> Motors;
        private DateTime _lastMotorCommandTime = DateTime.MinValue;
        private const int STOP_MOTOR_AFTER_TIMEOUT = 500;

        public async Task Initialize(byte addr = 0x60,
                                     int freq = 1600)
        {
            _i2caddress = addr;
            _frequency = freq;
            Motors = new List<Motor>
            {
                new Motor(this, 0),
                new Motor(this, 1),
                new Motor(this, 2),
                new Motor(this, 3)
            };

            PwmController = new PwmController(addr);
            await PwmController.Initialize();
            PwmController.SetDesiredFrequency(_frequency);

            Move(new MotorCommand
            {
                ForwardBackward = true,
                Speed = 0
            });
        }

        public void SetPin(int pin, int value)
        {
            if (pin < 0 || pin > 15)
            {
                throw new Exception("PWM pin must be between 0 and 15 inclusive");
            }

            if (value != 0 && value != 1)
            {
                throw new Exception("Pin value must be 0 or 1!");
            }

            if (value == 0)
            {
                PwmController.SetPwm((byte)pin, 4096, 0);
            }
            else if (value == 1)
            {
                PwmController.SetPwm((byte)pin, 0, 4096);
            }
        }

        public Motor GetMotor(int num)
        {
            if (num < 1 || num > 4)
            {
                throw new Exception("MotorHAT Motor must be between 1 and 4 inclusive");
            }

            return Motors[num - 1];
        }

        public void Move(MotorCommand motorCommand)
        {
            //Maximum speed from 0 to 255
            var maximumSpeed = 255;
            
            var motorLeft = GetMotor(4);
            var motorRight = GetMotor(1);

            //Stop
            if (motorCommand.Speed == 0)
            {
                motorLeft.SetSpeed(0);
                motorRight.SetSpeed(0);

                motorLeft.Run(MotorAction.RELEASE);
                motorRight.Run(MotorAction.RELEASE);
            }
            //Full right
            else if (motorCommand.FullRight)
            {
                var carSpeedFull = (int)Math.Round(motorCommand.Speed * maximumSpeed, 0);

                if (motorCommand.ForwardBackward)
                {
                    motorLeft.Run(MotorAction.FORWARD);
                    motorRight.Run(MotorAction.BACKWARD);

                    motorLeft.SetSpeed(carSpeedFull);
                    motorRight.SetSpeed(carSpeedFull);
                }
                else
                {
                    motorLeft.Run(MotorAction.BACKWARD);
                    motorRight.Run(MotorAction.FORWARD);

                    motorLeft.SetSpeed(carSpeedFull);
                    motorRight.SetSpeed(carSpeedFull);
                }
            }
            //Full left
            else if (motorCommand.FullLeft)
            {
                var carSpeedFull = (int)Math.Round(motorCommand.Speed * maximumSpeed, 0);

                if (motorCommand.ForwardBackward)
                {
                    motorLeft.Run(MotorAction.BACKWARD);
                    motorRight.Run(MotorAction.FORWARD);

                    motorLeft.SetSpeed(carSpeedFull);
                    motorRight.SetSpeed(carSpeedFull);
                }
                else
                {
                    motorLeft.Run(MotorAction.FORWARD);
                    motorRight.Run(MotorAction.BACKWARD);

                    motorLeft.SetSpeed(carSpeedFull);
                    motorRight.SetSpeed(carSpeedFull);
                }
            }
            //Slightly left
            else if (motorCommand.RightLeft < 0)
            {
                var carSpeedFull = (int)Math.Round(motorCommand.Speed * maximumSpeed, 0);
                var carSpeedSlow = (int)Math.Round(motorCommand.Speed * (1 - Math.Abs(motorCommand.RightLeft)) * maximumSpeed, 0);

                if (motorCommand.ForwardBackward)
                {
                    motorLeft.Run(MotorAction.FORWARD);
                    motorRight.Run(MotorAction.FORWARD);
                }
                else
                {
                    motorLeft.Run(MotorAction.BACKWARD);
                    motorRight.Run(MotorAction.BACKWARD);
                }

                motorLeft.SetSpeed(carSpeedSlow);
                motorRight.SetSpeed(carSpeedFull);
            }
            //Slightly right
            else if (motorCommand.RightLeft > 0)
            {
                var carSpeedFull = (int)Math.Round(motorCommand.Speed * maximumSpeed, 0);
                var carSpeedSlow = (int)Math.Round(motorCommand.Speed * (1 - Math.Abs(motorCommand.RightLeft)) * maximumSpeed, 0);

                if (motorCommand.ForwardBackward)
                {
                    motorLeft.Run(MotorAction.FORWARD);
                    motorRight.Run(MotorAction.FORWARD);
                }
                else
                {
                    motorLeft.Run(MotorAction.BACKWARD);
                    motorRight.Run(MotorAction.BACKWARD);
                }

                motorLeft.SetSpeed(carSpeedFull);
                motorRight.SetSpeed(carSpeedSlow);
            }
            //Forward or backward
            else if (motorCommand.RightLeft == 0)
            {
                var carSpeedFull = (int)Math.Round(motorCommand.Speed * maximumSpeed, 0);

                if (motorCommand.ForwardBackward)
                {
                    motorLeft.Run(MotorAction.FORWARD);
                    motorRight.Run(MotorAction.FORWARD);
                }
                else
                {
                    motorLeft.Run(MotorAction.BACKWARD);
                    motorRight.Run(MotorAction.BACKWARD);
                }

                motorLeft.SetSpeed(carSpeedFull);
                motorRight.SetSpeed(carSpeedFull);
            }
        }

        /// <summary>
        /// Move motors and stop motors after timeout when method is not called again within the timeout
        /// </summary>
        /// <param name="motorCommand"></param>
        public void MoveAndStopAfterTimeout(MotorCommand motorCommand)
        {
            Move(motorCommand);

            var motorCommandTime = DateTime.Now;
            _lastMotorCommandTime = motorCommandTime;

            Task.Run(async () => { await StopAfterTimeout(motorCommandTime); });
        }

        private async Task StopAfterTimeout(DateTime motorCommandTime)
        {
            await Task.Delay(STOP_MOTOR_AFTER_TIMEOUT);

            if (_lastMotorCommandTime <= motorCommandTime)
            {
                Move(new MotorCommand
                {
                    Speed = 0
                });
            }
        }
    }
}
