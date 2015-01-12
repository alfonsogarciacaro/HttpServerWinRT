using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace HttpServerWinRT
{
    public static class HttpStaticFileControllerWinRT
    {
        // TODO: Add more defaults?
        public const string DefaultFile = "/index.html";

        // Documentation recommends not to use buffers larger than 80KB (though this is for managed streams)
        // http://msdn.microsoft.com/en-us/library/windows/apps/hh994634.aspx
        public const int DefaultBufferSize = 81920;

        public static Func<HttpRequest, HttpResponse, Task> Create(StorageFolder dataFolder)
        {
            return async (request, response) =>
            {
                string filePath = request.ReqLine.Uri == "/" ? DefaultFile : request.ReqLine.Uri;
                filePath = filePath.Replace('/', '\\');
                var file = await dataFolder.GetFileAsync(filePath);

                await response.WriteHeadersAsync(
                    HttpStatus.OK,
                    Tuple.Create(HttpHeader.ContentType, file.ContentType),
                    Tuple.Create(HttpHeader.ContentLength, (await file.GetBasicPropertiesAsync()).Size.ToString())
                );

                var stream = ((StreamWriterAsync)response.Stream).Stream;
                var buffer = new Windows.Storage.Streams.Buffer(DefaultBufferSize);
                using (var fs = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    do
                    {
                        await fs.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.ReadAhead);
                        await stream.WriteAsync(buffer);
                    } while (buffer.Length == buffer.Capacity);
                }
            };
        }
    }
}
