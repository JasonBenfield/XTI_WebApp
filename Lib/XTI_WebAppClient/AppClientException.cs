using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace XTI_WebAppClient
{
    public sealed class AppClientException : Exception
    {
        public AppClientException(string url, HttpStatusCode statusCode, IEnumerable<ErrorModel> errors)
            : base(formatErrorMessage(url, statusCode, errors))
        {
            Url = url;
            StatusCode = statusCode;
            Errors = errors;
        }

        private static string formatErrorMessage(string url, HttpStatusCode statusCode, IEnumerable<ErrorModel> errors)
        {
            var joinedErrors = string.Join("\r\n", errors.Select(e => e.Format()));
            return $"{url}\r\n{statusCode}\r\n{joinedErrors}";
        }

        public string Url { get; }
        public HttpStatusCode StatusCode { get; }
        public IEnumerable<ErrorModel> Errors { get; }
    }
}
