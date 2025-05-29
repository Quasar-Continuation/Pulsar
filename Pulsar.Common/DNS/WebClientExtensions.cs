using System;
using System.Net;

namespace Pulsar.Common.DNS
{
    /// <summary>  
    /// WebClient with timeout capability  
    /// </summary>  
    internal class TimeoutWebClient : WebClient
    {
        private readonly int _timeout;

        public TimeoutWebClient(int timeout)
        {
            _timeout = timeout;
            this.Proxy = null;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);

            if (request != null)
            {
                request.Timeout = _timeout;
            }

            return request;
        }
    }

    /// <summary>
    /// Extension methods for WebClient
    /// </summary>
    internal static class WebClientExtensions
    {
        /// <summary>
        /// Creates a WebClient with the specified timeout in milliseconds
        /// </summary>
        public static WebClient WithTimeout(int timeout)
        {
            return new TimeoutWebClient(timeout);
        }
    }
}