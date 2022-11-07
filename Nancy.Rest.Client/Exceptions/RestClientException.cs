using System;
using System.Net;

namespace Nancy.Rest.Client.Exceptions
{
    public class RestClientException : Exception
    {
        private static string _className;
        public HttpStatusCode Status { get; private set; }
        public string Content { get; private set; }


        public RestClientException(HttpStatusCode statuscode, string message, string content) : base(message)
        {
            Content = content;
            Status = statuscode;
        }

        private static string ClassName => _className ?? (_className = typeof(RestClientException).ToString());

        public override string ToString()
        {
            var str1 = Message;
            var str2 = string.IsNullOrEmpty(str1) ? ClassName : ClassName + ": " + str1;

            var str3 = Content;
            if (!string.IsNullOrEmpty(str3))
                str2 = str2 + Environment.NewLine + str3;

            var stackTrace = StackTrace;
            if (stackTrace != null)
                str2 = str2 + Environment.NewLine + stackTrace;

            return str2;
        }
    }
}
