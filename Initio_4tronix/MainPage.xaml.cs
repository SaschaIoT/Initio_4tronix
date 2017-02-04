using Initio_4tronix.CarController;
using Initio_4tronix.Devices;
using Initio_4tronix.Web;
using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Initio_4tronix
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            Loaded += PageLoaded;
        }

        private async void PageLoaded(object sender, RoutedEventArgs eventArgs)
        {
            await Initialize();
        }

        private async Task Initialize()
        {
            if (LightningProvider.IsLightningEnabled)
            {
                LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();
            }
            else
            {
                throw new Exception("Lightning drivers not enabled. Please enable Lightning drivers.");
            }

            var motorController = new MotorController();
            await motorController.Initialize();

            var servoController = new ServoController();
            await servoController.Initialize();

            new GamepadController(motorController, servoController);

            var camera = new Camera();
            await camera.Initialize();

            var httpServer = new HttpServer(motorController, servoController, camera);
            httpServer.Start();
        }
    }
}
