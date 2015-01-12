using System;
using System.Collections.Generic;
using System.IO;

namespace HttpServerWinRT
{
    public static class HttpUtil
    {
        private static readonly IDictionary<string, string> MIME = new Dictionary<string, string>()
        {
            {".css", "text/css"},
            {".gif", "image/gif"},
            {".html", "text/html"},
            {".jpg", "image/jpeg"},
            {".js", "application/javascript"},
            {".json", "application/json"},
            {".mp3", "audio/mpeg"},
            {".png", "image/png"},
            {".svg", "image/svg+xml"},
            {".swf", "application/x-shockwave-flash"},
            {".ttf", "application/x-font-ttf"},
            {".txt", "text/plain"},
            {".woff", "application/font-woff"},
            {".xml", "application/xml"},
            {".zip", "application/zip"},
        };

        /// <summary>
        /// Returns null if the file type is unknown.
        /// </summary>
        public static string GetMimeType(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLower();
            return MIME.ContainsKey(ext) ? MIME[ext] : null;
        }

        public static string FromHttpStatusToString(HttpStatus status)
        {
            switch (status)
            {
                case HttpStatus.OK:
                    return "200 OK";
                case HttpStatus.NotFound:
                    return "404 Not Found";
                case HttpStatus.Continue:
                    return "100 Continue";
                default:
                    throw new NotImplementedException();
            }
        }

        public static string FromHttpHeaderToString(HttpHeader header)
        {
            switch (header)
            {
                case HttpHeader.ContentType:
                    return "Content-Type";
                case HttpHeader.ContentLength:
                    return "Content-Length";
                case HttpHeader.Connection:
                    return "Connection";
                case HttpHeader.CacheControl:
                    return "Cache-Control";
                case HttpHeader.Pragma:
                    return "Pragma";
                case HttpHeader.Expires:
                    return "Expires";
                case HttpHeader.AccessControlAllowOrigin:
                    return "Access-Control-Allow-Origin";
                case HttpHeader.AccessControlAllowHeaders:
                    return "Access-Control-Allow-Headers";
                case HttpHeader.AccessControlAllowMethods:
                    return "Access-Control-Allow-Methods";
                case HttpHeader.AccessControlAllowCredentials:
                    return "Access-Control-Allow-Credentials";
                default:
                    throw new NotImplementedException();
            }
        }

        public static HttpMethod FromStringToHttpMethod(string httpMethod)
        {
            switch (httpMethod)
            {
                case "GET":
                    return HttpMethod.GET;
                case "PUT":
                    return HttpMethod.PUT;
                case "POST":
                    return HttpMethod.POST;
                case "DELETE":
                    return HttpMethod.DELETE;
                case "OPTIONS":
                    return HttpMethod.OPTIONS;
                default:
                    throw new Exception("Unknown HTTP method");
            }
        }
    }

    // TODO: Complete
    public enum HttpMethod
    {
        GET, POST, PUT, DELETE, OPTIONS
    }

    // TODO: Complete
    public enum HttpStatus
    {
        OK, NotFound, Continue
    }

    // TODO: Complete
    public enum HttpHeader
    {
        ContentType,
        ContentLength,
        Connection,
        CacheControl,
        Pragma,
        Expires,
        AccessControlAllowOrigin,
        AccessControlAllowHeaders,
        AccessControlAllowMethods,
        AccessControlAllowCredentials
    }
}
