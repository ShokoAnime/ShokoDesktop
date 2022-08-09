using System;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace Nancy.Rest.Client.Rest
{
    public class Request
    {
        public Uri BaseUri { get; set; }
        public HttpMethod Method { get; set; }
        public string Path { get; set; }
        public object BodyObject { get; set; }
        public JsonSerializerSettings SerializerSettings { get; set; }
        public Type ReturnType { get; set; }
        public TimeSpan Timeout { get; set; }
        public bool IsWWWFormUrlencoded { get; set; }
        public void AddQueryParamater(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("The query parameter name cannot be NULL or empty.");
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException("The query parameter value cannot be NULL or empty.");
            string separator = "&";
            if (!Path.Contains("?"))
                separator = "?";
            Path=Path + separator + WebUtility.UrlEncode(name) + "=" + WebUtility.UrlEncode(value);
        }
    }
}
