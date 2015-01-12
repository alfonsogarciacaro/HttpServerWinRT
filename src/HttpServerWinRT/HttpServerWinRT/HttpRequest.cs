using HttpMultipartParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpServerWinRT
{
    public class HttpRequest
    {
        public HttpRequestLine ReqLine { get; private set; }
        public IDictionary<string, string> Headers { get; private set; }
        public HttpRequestBody Body { get; private set; }

        HttpRequest() { }

        public bool ContainsHeader(HttpHeader header)
        {
            var str = HttpUtil.FromHttpHeaderToString(header);
            return this.Headers.ContainsKey(str);
        }

        public string GetHeader(HttpHeader header)
        {
            var str = HttpUtil.FromHttpHeaderToString(header);
            return this.Headers.ContainsKey(str) ? this.Headers[str] : null;
        }

        public static async Task<HttpRequest> Create(IInputStreamAsync stream)
        {
            var req = new HttpRequest();
            req.ReqLine = new HttpRequestLine(await stream.ReadLineAsync());

            req.Headers = new Dictionary<string, string>();
            var header = await stream.ReadLineAsync();
            while (!string.IsNullOrEmpty(header))
            {
                var parts = header.Split(new string[] { ": " }, StringSplitOptions.None);
                if (parts.Length == 2)
                    req.Headers.Add(parts[0], parts[1]);
                header = await stream.ReadLineAsync();
            }

            req.Body =
                req.ReqLine.Method == HttpMethod.GET || req.ReqLine.Method == HttpMethod.OPTIONS
                ? HttpRequestBody.Empty()
                : req.ContainsHeader(HttpHeader.ContentType) && req.GetHeader(HttpHeader.ContentType).Contains("multipart/form-data")
                    ? await HttpRequestBody.Multipart(stream)
                    : await HttpRequestBody.Text(stream);

            return req;
        }   
    }

    public class HttpRequestLine
    {
        public HttpMethod Method { get; private set; }
        public string Uri { get; private set; }
        public string UriWithParameters { get; private set; }
        public string Raw { get; private set; }
        public HttpUriParameters UriParameters { get; private set; }

        public HttpRequestLine(string requestLine)
        {
            var requestParts = requestLine.Split(' ');
            var queryParts = requestParts[1].Split('?');

            Raw = requestLine;
            Method = HttpUtil.FromStringToHttpMethod(requestParts[0]);
            UriWithParameters = requestParts[1];

            Uri = queryParts[0];
            UriParameters = queryParts.Length < 2
                ? new HttpUriParameters()
                : new HttpUriParameters(queryParts[1]);
        }
    }

    public class HttpUriParameters
    {
        IList<KeyValuePair<string, string>> parameters;

        public bool Any(Func<string,bool> predicate)
        {
            return parameters == null || parameters.Count == 0
                ? false
                : parameters.Any(x => predicate(x.Key));
        }

        public bool Any(string key)
        {
            return parameters == null || parameters.Count == 0
                ? false
                : parameters.Any(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<string> Where(Func<string,bool> predicate)
        {
            if (parameters == null || parameters.Count == 0)
                yield break;

            foreach (var p in parameters.Where(x => predicate(x.Key)))
                yield return p.Value;
        }


        public IEnumerable<string> Where(string key)
        {
            if (parameters == null || parameters.Count == 0)
                yield break;

            foreach (var p in parameters.Where(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase)))
                yield return p.Value;
        }

        public string SingleOrDefault(string key)
        {
            if (parameters == null || parameters.Count == 0)
                return null;

            var p = parameters.SingleOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            return p.Equals(default(KeyValuePair<string, string>))
                ? null
                : p.Value;
        }

        public HttpUriParameters() { }

        public HttpUriParameters(string rawParameters)
        {
            parameters = rawParameters
                .Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(qp =>
                {
                    var qps = qp.Split('=');
                    return new KeyValuePair<string, string>(qps[0], qps.Length > 1
                        ? System.Net.WebUtility.UrlDecode(qps[1])
                        : null);
                })
                .ToList();
        }

        public static bool TryParse(string rawParameters, out HttpUriParameters formattedParameters)
        {
            try
            {
                formattedParameters = new HttpUriParameters(rawParameters);
                return true;
            }
            catch
            {
                formattedParameters = null;
                return false;
            }
        }
    }

    public class HttpRequestBody
    {
        enum BodyType
        {
            Empty, Text, Multipart
        }

        BodyType bodyType;
        object body;

        HttpRequestBody(BodyType bodyType, object body)
        {
            this.bodyType = bodyType;
            this.body = body;
        }

        public static HttpRequestBody Empty()
        {
            return new HttpRequestBody(BodyType.Empty, null);
        }

        public static async Task<HttpRequestBody> Text(IInputStreamAsync stream)
        {
            var body = await stream.ReadToEndAsync();

            return !string.IsNullOrEmpty(body)
                ? new HttpRequestBody(BodyType.Text, body)
                : new HttpRequestBody(BodyType.Empty, null);
        }

        public static async Task<HttpRequestBody> Multipart(IInputStreamAsync stream)
        {
            return new HttpRequestBody(BodyType.Multipart, await MultipartFormDataParser.CreateAsync(stream));
        }

        public void Match(Action emptyCase, Action<string> textCase, Action<MultipartFormDataParser> multipartCase)
        {
            switch (bodyType)
            {
                case BodyType.Empty:
                    emptyCase();
                    break;
                case BodyType.Text:
                    textCase((string)body);
                    break;
                case BodyType.Multipart:
                    multipartCase((MultipartFormDataParser)body);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public T Match<T>(Func<T> emptyCase, Func<string, T> textCase, Func<MultipartFormDataParser, T> multipartCase)
        {
            switch (bodyType)
            {
                case BodyType.Empty:
                    return emptyCase();
                case BodyType.Text:
                    return textCase((string)body);
                case BodyType.Multipart:
                    return multipartCase((MultipartFormDataParser)body);
                default:
                    throw new NotImplementedException();
            }
        }
    }

}
