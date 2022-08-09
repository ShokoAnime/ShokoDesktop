using System;
using System.Net;

namespace Nancy.Rest.Client.Exceptions
{
    public class RestClientException : Exception
    {
        public HttpStatusCode Status { get; private set; }
        public string Content { get; private set; }


        public RestClientException(HttpStatusCode statuscode, string message, string content) : base(message)
        {
            Content = content;
            Status = statuscode;
        }
    }
}
