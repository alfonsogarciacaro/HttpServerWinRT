using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServerWinRT
{
    public class HttpResponse
    {
        public IOutputStreamAsync Stream { get; private set; }


        public HttpResponse(IOutputStreamAsync stream)
        {
            this.Stream = stream;
        }

        public async Task WriteHeadersAsync(HttpStatus status, params Tuple<HttpHeader, string>[] headers)
        {
            var builder = new StringBuilder()
                .Append(string.Format("HTTP/1.1 {0}\r\n", HttpUtil.FromHttpStatusToString(status)));

            if (headers != null)
                foreach (var header in headers.Where(h => !string.IsNullOrWhiteSpace(h.Item2)))
                    builder.Append(string.Format("{0}: {1}\r\n",
                                    HttpUtil.FromHttpHeaderToString(header.Item1), header.Item2));

            if (headers == null || !headers.Any(h => h.Item1 == HttpHeader.Connection))
                builder.Append("Connection: close\r\n");

            builder.Append("\r\n");
            await this.Stream.WriteStringAsync(builder.ToString());
        }

        public async Task WriteStringAsync(string str)
        {
            await this.Stream.WriteStringAsync(str);
        }

        public async Task WriteBytesAsync(byte[] bytes)
        {
            await this.Stream.WriteBytesAsync(bytes);
        }
    }
}
