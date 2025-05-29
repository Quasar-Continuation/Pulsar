using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Pulsar.Common.DNS
{
    /// <summary>
    /// Handles fetching raw text content from pastebin or similar raw text hosting services.
    /// </summary>
    public class PastebinFetcher
    {
        private readonly string _url;
        private string _cachedContent;
        private DateTime _lastFetchTime;
        private int _requestCount;
        private const int REQUEST_THRESHOLD = 100;
        private bool _hadError;

        /// <summary>
        /// Gets whether it's time to refresh content from the source.
        /// </summary>
        public bool ShouldRefresh => _requestCount >= REQUEST_THRESHOLD || (_hadError && _cachedContent == null);

        public PastebinFetcher(string url)
        {
            _url = url;
            _lastFetchTime = DateTime.MinValue;
            _requestCount = 0;
            _hadError = false;
        }

        /// <summary>
        /// Fetches the raw content from the URL.
        /// </summary>
        /// <returns>The raw content from the URL.</returns>
        public string FetchContent()
        {
            _requestCount++;

            if (_cachedContent != null && _requestCount < REQUEST_THRESHOLD)
            {
                return _cachedContent;
            }

            try
            {
                _requestCount = 0;
                _hadError = false;

                using (WebClient client = new TimeoutWebClient(5000))
                {
                    _cachedContent = client.DownloadString(_url);
                    _lastFetchTime = DateTime.Now;
                    return _cachedContent;
                }
            }
            catch (Exception)
            {
                _hadError = true;
                
                if (_cachedContent != null)
                {
                    return _cachedContent;
                }
                
                return string.Empty;
            }
        }

        /// <summary>
        /// Asynchronously fetches the raw content from the URL.
        /// </summary>
        /// <returns>A task containing the raw content from the URL.</returns>
        public async Task<string> FetchContentAsync()
        {
            _requestCount++;

            if (_cachedContent != null && _requestCount < REQUEST_THRESHOLD)
            {
                return _cachedContent;
            }

            try
            {
                _requestCount = 0;
                _hadError = false;

                using (WebClient client = new TimeoutWebClient(5000)) // 5 second timeout
                {
                    _cachedContent = await client.DownloadStringTaskAsync(_url);
                    _lastFetchTime = DateTime.Now;
                    return _cachedContent;
                }
            }
            catch (Exception)
            {
                _hadError = true;
                
                if (_cachedContent != null)
                {
                    return _cachedContent;
                }
                
                return string.Empty;
            }
        }
    }
}