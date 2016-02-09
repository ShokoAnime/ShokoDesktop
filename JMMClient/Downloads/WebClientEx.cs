using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace JMMClient.Downloads
{
    public class WebClientEx : WebClient
    {
        public CookieContainer CookieContainer { get; private set; }

        public WebClientEx()
        {
            CookieContainer = new CookieContainer();
        }

        public WebClientEx(System.Net.CookieContainer cookie_container)
        {
            CookieContainer = cookie_container;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = CookieContainer;
            }
            return request;
        }
    }
}
