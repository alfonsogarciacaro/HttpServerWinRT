using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace HttpServerWinRT
{
    public class HttpServerWinRT : IDisposable
    {
        StreamSocketListener listener = null;
        Func<HttpRequest, HttpResponse, Task> defaultController;
        IDictionary<string, Func<HttpRequest, HttpResponse, Task>> controllers;

        HttpServerWinRT(
            Func<HttpRequest, HttpResponse, Task> defCtrl,
            IDictionary<string, Func<HttpRequest, HttpResponse, Task>> ctrls)
        {
            controllers = ctrls;             // TODO: Make keys lower case?
            defaultController = defCtrl;
            listener = new StreamSocketListener();
            listener.ConnectionReceived += ProcessRequestAsync;
        }

        public void Dispose()
        {
            if (listener != null)
                listener.Dispose();
        }

        public static async Task<HttpServerWinRT> Start(
            int port,
            Func<HttpRequest, HttpResponse, Task> defaultController,
            IDictionary<string, Func<HttpRequest, HttpResponse, Task>> controllers)
        {
            var server = new HttpServerWinRT(defaultController, controllers);
            await server.listener.BindServiceNameAsync(port.ToString());
            return server;
        }

        async void ProcessRequestAsync(
            StreamSocketListener sender,
            StreamSocketListenerConnectionReceivedEventArgs e)
        {
            try
            {
                using (var input = await StreamReaderAsync.Create(e.Socket.InputStream))
                using (var output = e.Socket.OutputStream)
                {
                    var response = new HttpResponse(new StreamWriterAsync(output));
                    var request = await HttpRequest.Create(input);

                    var query = request.ReqLine.Uri.ToLower();
                    var controller =
                        controllers.FirstOrDefault(kv => query.Contains(kv.Key)).Value
                        ?? defaultController;
                    await controller(request, response);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);

                // If this is an unknown status it means that the error is fatal and retry will likely fail.
                //if (SocketError.GetStatus(ex.HResult) == SocketErrorStatus.Unknown) { throw; }
            }
        }
    }
}
