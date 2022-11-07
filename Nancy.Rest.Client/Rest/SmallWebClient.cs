using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Nancy.Rest.Client.Exceptions;
using Newtonsoft.Json;

namespace Nancy.Rest.Client.Rest
{
    public class SmallWebClient
    {

        public static async Task<object> RestRequest(Request req, IWebProxy proxy=null)
        {
            var handler=new HttpClientHandler();
            if (proxy != null)
            {
                handler.Proxy = proxy;
                handler.UseProxy = true;
            }
            using (var client = new HttpClient(handler, true))
            {
                var returnasstream = false;
                var accept = "application/json";
                if (req.ReturnType.IsAssignableFrom(typeof(Stream)))
                {
                    returnasstream = true;
                    accept = "*/*";
                }
                client.BaseAddress = req.BaseUri;
                client.Timeout = req.Timeout;
                var request = new HttpRequestMessage(req.Method, req.Path);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));
                if (req.BodyObject != null)
                {
                    if (req.BodyObject.GetType().IsAssignableFrom(typeof(Stream)))
                    {
                        request.Content = new StreamContent((Stream) req.BodyObject);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                    }
                    else if (req.IsWWWFormUrlencoded)
                    {
                        request.Content = new StringContent((string)req.BodyObject,Encoding.UTF8);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
                    }
                    else
                    {                        
                        var str = JsonConvert.SerializeObject(req.BodyObject, Formatting.None);
                        if (!string.IsNullOrEmpty(str))
                        {
                            request.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(str));
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        }
                    }
                }

                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                using (var content = response.Content)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var message = await content.ReadAsStringAsync();
                        throw new RestClientException(response.StatusCode, response.ReasonPhrase, message);
                    }

                    if (returnasstream)
                        return await content.ReadAsStreamAsync();
                    if (req.ReturnType == typeof(void))
                        return null;
                    return JsonConvert.DeserializeObject(await content.ReadAsStringAsync(), req.ReturnType,
                        req.SerializerSettings);
                }
            }
        }
    }
}
