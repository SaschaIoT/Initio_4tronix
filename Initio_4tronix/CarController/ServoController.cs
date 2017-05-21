using System;
using System.Threading.Tasks;

namespace Initio_4tronix.CarController
{
    public class ServoController
    {
        private PwmController _pwmController;

        //TODO: Adjust vertical servo min position
        private const ushort SERVO_MIN_POSITION = 150;
        //TODO: Vertical servo max position
        private const ushort SERVO_MAX_POSITION = 430;

        //TODO: Initial vertical servo position
        private ushort _tiltServoCurrentPosition = 290;
        
        public async Task Initialize()
        {
            _pwmController = new PwmController(0x40);
            await _pwmController.Initialize();
            _pwmController.SetDesiredFrequency(60);

            //TODO: Initial horizontal servo position
            _pwmController.SetPwm(0, 0, 305);
            //Initial vertical servo position
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
                //Up or down vertical servo position
                _pwmController.SetPwm(1, 0, _tiltServoCurrentPosition);
            }
        }
    }
}
