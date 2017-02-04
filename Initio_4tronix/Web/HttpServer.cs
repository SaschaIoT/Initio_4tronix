using Initio_4tronix.CarController;
using Initio_4tronix.Devices;
using Initio_4tronix.Helper;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.System;

namespace Initio_4tronix.Web
{
    public sealed class HttpServer
    {
        private const uint BUFFER_SIZE = 3024;
        private readonly StreamSocketListener _listener;

        //Dependency objects
        private MotorController _motorController;
        private ServoController _servoController;
        private Camera _camera;

        public HttpServer(MotorController motorController,
                          ServoController servoController,
                          Camera camera)
        {
            _motorController = motorController;
            _servoController = servoController;
            _camera = camera;

            _listener = new StreamSocketListener();
            _listener.ConnectionReceived += ProcessRequest;
            _listener.Control.KeepAlive = false;
            _listener.Control.NoDelay = false;
            _listener.Control.QualityOfService = SocketQualityOfService.LowLatency;
        }

        public async void Start()
        {
            await _listener.BindServiceNameAsync(80.ToString());
        }

        private async void ProcessRequest(StreamSocketListener streamSocktetListener, StreamSocketListenerConnectionReceivedEventArgs eventArgs)
        {
            try
            {
                var socket = eventArgs.Socket;

                //Read request
                var httpRequest = await ReadRequest(socket);

                //Write Response
                await WriteResponse(httpRequest, socket.OutputStream);

                socket.InputStream.Dispose();
                socket.OutputStream.Dispose();
                socket.Dispose();
            }
            catch (Exception) { }
        }

        private async Task<HttpRequest> ReadRequest(StreamSocket socket)
        {
            var request = string.Empty;

            using (var inputStream = socket.InputStream)
            {
                var data = new byte[BUFFER_SIZE];
                var buffer = data.AsBuffer();

                var startReadRequest = DateTime.Now;
                while (!HttpGetRequestHasUrl(request))
                {
                    if (DateTime.Now.Subtract(startReadRequest) >= TimeSpan.FromMilliseconds(5000))
                    {
                        throw new TaskCanceledException("Request timeout.");
                    }

                    var inputStreamReadTask = inputStream.ReadAsync(buffer, BUFFER_SIZE, InputStreamOptions.Partial);
                    var timeout = TimeSpan.FromMilliseconds(1000);
                    await TaskHelper.CancelTaskAfterTimeout(ct => inputStreamReadTask.AsTask(ct), timeout);

                    request += Encoding.UTF8.GetString(data, 0, data.Length);
                }
            }

            var requestMethod = request.Split('\n')[0];
            var requestParts = requestMethod.Split(' ');
            var relativeUrl = requestParts.Length > 1 ? requestParts[1] : string.Empty;

            return new HttpRequest { Request = request, RelativeUrl = relativeUrl.ToLowerInvariant() };
        }

        private async Task WriteResponse(HttpRequest httpRequest, IOutputStream outputStream)
        {
            //Get javascript files
            if (httpRequest.RelativeUrl.StartsWith("/javascript"))
            {
                await HttpResponse.WriteResponseFile(ToFolderPath(httpRequest.RelativeUrl), HttpContentType.JavaScript, outputStream);
            }
            //Get css style files
            else if (httpRequest.RelativeUrl.StartsWith("/styles"))
            {
                await HttpResponse.WriteResponseFile(ToFolderPath(httpRequest.RelativeUrl), HttpContentType.Css, outputStream);
            }
            //Get images
            else if (httpRequest.RelativeUrl.StartsWith("/images"))
            {
                await HttpResponse.WriteResponseFile(ToFolderPath(httpRequest.RelativeUrl), HttpContentType.Png, outputStream);
            }
            //Get current camera frame
            else if (httpRequest.RelativeUrl.StartsWith("/videoframe"))
            {
                if (_camera.Frame != null)
                {
                    HttpResponse.WriteResponseFile(_camera.Frame, HttpContentType.Jpeg, outputStream);
                }
                else
                {
                    HttpResponse.WriteResponseError("Not camera fram available. Maybe there is an error or camera is not started.", outputStream);
                }
            }
            //Shutdown
            else if (httpRequest.RelativeUrl.StartsWith("/shutdown"))
            {
                await ProcessLauncher.RunToCompletionAsync(@"CmdWrapper.exe", "\"shutdown -s -t 0\"");
            }
            //Motor and servo command
            else if (httpRequest.RelativeUrl.StartsWith("/motorservocommand"))
            {
                var httpRequestContent = HttpRequestContentHelper.GetContentAsJson(httpRequest.Request);
                if (!httpRequestContent.Success)
                {
                    HttpResponse.WriteResponseError("Request contains no content.", outputStream);
                }

                var content = httpRequestContent.Content;

                var forward = content["forward"].GetBoolean();
                var backward = content["backward"].GetBoolean();

                var motorCommand = new MotorCommand
                {
                    ForwardBackward = forward || !backward,
                    FullLeft = content["fullLeft"].GetBoolean(),
                    FullRight = content["fullRight"].GetBoolean(),
                    Speed = content["speed"].GetBoolean() ? 1 : 0
                };

                var servoCommand = new ServoCommand
                {
                    Up = content["up"].GetBoolean(),
                    Down = content["down"].GetBoolean(),
                    Speed = 7
                };

                _motorController.MoveAndStopAfterTimeout(motorCommand);
                _servoController.Move(servoCommand);

                HttpResponse.WriteResponseOk(outputStream);
            }
            //Get Desktop.html page
            else if (httpRequest.RelativeUrl.StartsWith("/desktop"))
            {
                await HttpResponse.WriteResponseFile(@"\Html\Desktop.html", HttpContentType.Html, outputStream);
            }
            //Get Mobile.html page
            else if (httpRequest.RelativeUrl.StartsWith("/mobile"))
            {
                await HttpResponse.WriteResponseFile(@"\Html\Mobile.html", HttpContentType.Html, outputStream);
            }
            //Get index.html page
            else
            {
                await HttpResponse.WriteResponseFile(@"\Html\Index.html", HttpContentType.Html, outputStream);
            }
        }

        private bool HttpGetRequestHasUrl(string httpRequest)
        {
            var regex = new Regex("^.*GET.*HTTP.*\\r\\n.*$", RegexOptions.Multiline);
            return regex.IsMatch(httpRequest.ToUpper());
        }
                
        private string ToFolderPath(string relativeUrl)
        {
            var folderPath = relativeUrl.Replace('/', '\\');
            return folderPath;
        }
    }
}
