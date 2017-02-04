using Initio_4tronix.Helper;
using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Pwm;

namespace Initio_4tronix.CarController
{
    public class ServoController
    {
        private PwmController _pwmController;
        
        private const ushort SERVO_MIN_POSITION = 150;
        private const ushort SERVO_MAX_POSITION = 430;

        private ushort _tiltServoCurrentPosition = 290;
        
        public async Task Initialize()
        {
            _pwmController = new PwmController(0x40);
            await _pwmController.Initialize();
            _pwmController.SetDesiredFrequency(60);

            _pwmController.SetPwm(0, 0, 340);
            _pwmController.SetPwm(1, 0, _tiltServoCurrentPosition);
        }

        public void Move(ServoCommand servoCommand)
        {
            if (servoCommand.Up)
            {
                if (SERVO_MIN_POSITION == _tiltServoCurrentPosition)
                {
                    return;
                }

                _tiltServoCurrentPosition -= servoCommand.Speed;

                if (_tiltServoCurrentPosition < SERVO_MIN_POSITION)
                {
                    _tiltServoCurrentPosition = SERVO_MIN_POSITION;
                }
            }
            else if (servoCommand.Down)
            {
                if (SERVO_MAX_POSITION == _tiltServoCurrentPosition)
                {
                    return;
                }

                _tiltServoCurrentPosition += servoCommand.Speed;

                if (_tiltServoCurrentPosition > SERVO_MAX_POSITION)
                {
                    _tiltServoCurrentPosition = SERVO_MAX_POSITION;
                }
            }

            if (servoCommand.Up || servoCommand.Down)
            {
                _pwmController.SetPwm(1, 0, _tiltServoCurrentPosition);
            }
        }
    }
}
