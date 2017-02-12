using Initio_4tronix.CarController;
using System;
using System.Threading.Tasks;
using Windows.Gaming.Input;
using Windows.System;

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

            var buttonDownTimeLong = TimeSpan.FromSeconds(5);
            var xRightShoulderButton = new GamepadButtonDown(buttonDownTimeLong, GamepadButtons.X, GamepadButtons.RightShoulder);

            while (_gamepad == gamepad)
            {
                var gamepadReadingTry = gamepad?.GetCurrentReading();
                if (!gamepadReadingTry.HasValue)
                    break;

                var gamepadReading = gamepadReadingTry.Value;
                
                var motorCommand = new MotorCommand(gamepadReading);
                var servoCommand = new ServoCommand(gamepadReading);

                if (!_carStopped || motorCommand.Speed != 0.0 || servoCommand.Speed != 0.0)
                {
                    _carStopped = false;

                    //Move motors and servos
                    _motorController.Move(motorCommand);
                    _servoController.Move(servoCommand);
                }

                if (motorCommand.Speed == 0 && servoCommand.Speed == 0)
                {
                    _carStopped = true;
                }

                //Shutdown
                var xRightShoulderButtonResult = xRightShoulderButton.UpdateGamepadButtonState(gamepadReading);
                if (xRightShoulderButtonResult.ButtonClicked)
                {
                    await ProcessLauncher.RunToCompletionAsync(@"CmdWrapper.exe", "\"shutdown -s -t 0\"");
                }

                await Task.Delay(25);
            }

            _isGamepadReadingStopped = true;
        }
    }
}
