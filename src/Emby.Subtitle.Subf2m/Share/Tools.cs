using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#if DEBUG
#endif

namespace Emby.Subtitle.SubF2M.Share
{

    public static class Tools
    {
        private static readonly HttpClient _httpClient;

        static Tools()
        {
            _httpClient = new HttpClient();
        }

        public static async Task<string> RequestUrl(string baseUrl, string path, HttpMethod method, object postData = null,
                    Dictionary<string, string> headers = null, int timeout = 10_000)
        {
            var retValue = string.Empty;
            try
            {
                if (!baseUrl.StartsWith("http://") && !baseUrl.StartsWith("https://"))
                    baseUrl = $"http://{baseUrl}";

                var message = new HttpRequestMessage(method, new Uri(new Uri(baseUrl), path));

                if (headers != null && headers.Count > 0)
                {
                    foreach (var header in headers)
                    {
                        message.Headers.Add(header.Key, header.Value);
                    }
                }

                var cancellationToken = new CancellationTokenSource();
                cancellationToken.CancelAfter(timeout);

                var res = await _httpClient.SendAsync(message, cancellationToken.Token);

                if (res == null)
                    return retValue;

                res.EnsureSuccessStatusCode();

                if (!res.IsSuccessStatusCode)
                {
                    /*var cnt = string.Empty;
                    if (res.Content != null)
                        cnt = await res.Content?.ReadAsStringAsync();*/

                    return retValue;
                }

                retValue = await res.Content.ReadAsStringAsync();

            }
            catch
            {
                //
            }

            return retValue;
        }

#if DEBUG
        public static async Task<T> RequestUrl<T>(string baseUrl, string path, HttpMethod method, object postData = null,
            Dictionary<string, string> headers = null, int timeout = 10_000)
        {
            var r = await RequestUrl(baseUrl, path, method, postData, headers, timeout);
            return string.IsNullOrWhiteSpace(r)
                ? default
                : JsonConvert.DeserializeObject<T>(r);
        }
#endif


    }
}