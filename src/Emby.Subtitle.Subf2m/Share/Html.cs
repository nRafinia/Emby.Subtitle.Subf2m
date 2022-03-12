using Emby.Subtitle.SubF2M.Extensions;
using MediaBrowser.Common;
using MediaBrowser.Common.Net;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Emby.Subtitle.SubF2M.Share
{
    public class Html
    {
        private readonly IHttpClient _httpClient;
        private readonly IApplicationHost _appHost;

        public Html(IHttpClient httpClient, IApplicationHost appHost)
        {
            _httpClient = httpClient;
            _appHost = appHost;
        }

        public async Task<string> Get(string url, CancellationToken cancellationToken)
        {
            //var html = await Tools.RequestUrl(domain, path, HttpMethod.Get);
            var html = await GetHtmlContent(url, cancellationToken);

            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            var headIndex = html.IndexOf("<head>", StringComparison.Ordinal);
            var headEnd = html.IndexOf("</head>", headIndex + 1, StringComparison.Ordinal);
            var endHeadBlock = headEnd - headIndex + 7;
            html = html.Remove(headIndex, endHeadBlock);

            var scIndex = html.IndexOf("<script", StringComparison.Ordinal);
            while (scIndex >= 0)
            {
                var scEnd = html.IndexOf("</script>", scIndex + 1, StringComparison.Ordinal);
                var end = scEnd - scIndex + 9;
                html = html.Remove(scIndex, end);
                scIndex = html.IndexOf("<script", StringComparison.Ordinal);
            }

            scIndex = html.IndexOf("&#", StringComparison.Ordinal);
            while (scIndex >= 0)
            {
                var scEnd = html.IndexOf(";", scIndex + 1, StringComparison.Ordinal);
                var end = scEnd - scIndex + 1;
                var word = html.Substring(scIndex, end);
                html = html.Replace(word, System.Net.WebUtility.HtmlDecode(word));
                scIndex = html.IndexOf("&#", StringComparison.Ordinal);
            }

            return html.FixHtml();
        }

        public HttpRequestOptions BaseRequestOptions(string url, CancellationToken cancellationToken)
            => new HttpRequestOptions
            {
                UserAgent = $"Emby/{_appHost?.ApplicationVersion}",
                Url = url,
                CancellationToken = cancellationToken
            };

        private async Task<string> GetHtmlContent(string url, CancellationToken cancellationToken)
        {
            var options = BaseRequestOptions(url, cancellationToken);

            using var response = await _httpClient.GetResponse(options);

            using var reader = new StreamReader(response.Content);
            var text = await reader.ReadToEndAsync();
            return text;
        }
    }
}