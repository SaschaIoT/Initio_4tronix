using Initio_4tronix.CarController;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Gaming.Input;

namespace Initio_4tronix.Devices
{
    public class GamepadController
    {
        private Gamepad _gamepad;

        private bool _carStopped;
        private volatile bool _isGamepadReadingStopped = true;
        private volatile bool _isGamepadVibrationStopped = true;

        //Dependencies
        private MotorController _motorController;
        private ServoController _servoController;

        public GamepadController(MotorController motorController, ServoController servoController)
        {
            _motorController = motorController;
            _servoController = servoController;

            Gamepad.GamepadAdded += GamepadAdded;
            Gamepad.GamepadRemoved += GamepadRemoved;
        }

        private async void GamepadAdded(object sender, Gamepad gamepad)
        {
            _gamepad = gamepad;
            
            await StartGamepadReading(gamepad);
        }

        private async void GamepadRemoved(object sender, Gamepad gamepad)
        {
            _gamepad = null;

            while (!_isGamepadReadingStopped || !_isGamepadVibrationStopped)
            {
                await Task.Delay(10);
            }

            _motorController.Move(new MotorCommand
            {
                Speed = 0
            });
        }

        private async Task StartGamepadReading(Gamepad gamepad)
        {
            _isGamepadReadingStopped = false;

            var buttonDownTimeMiddle = TimeSpan.FromSeconds(2);

            while (_gamepad == gamepad)
            {
                //gamepad variable could be null
                var gamepadReadingTry = gamepad?.GetCurrentReading();
                if (!gamepadReadingTry.HasValue)
                    break;

                var gamepadReading = gamepadReadingTry.Value;
                
                var motorCommand = new MotorCommand(gamepadReading);
                var servoCommand = new ServoCommand(gamepadReading);

                if (!_carStopped || motorCommand.Speed != 0.0 || servoCommand.Speed != 0.0)
                {
                    _carStopped = false;

                    _motorController.Move(motorCommand);
                    _servoController.Move(servoCommand);
                }

                if (motorCommand.Speed == 0 && servoCommand.Speed == 0)
                {
                    _carStopped = true;
                }

                await Task.Delay(25);
            }

            _isGamepadReadingStopped = true;
        }
    }
}
