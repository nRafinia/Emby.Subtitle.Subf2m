using Emby.Subtitle.SubF2M.Extensions;
using Emby.Subtitle.SubF2M.Share;
using MediaBrowser.Common;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Model.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Emby.Subtitle.SubF2M.Providers
{
    public class Subtitle
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly IApplicationHost _appHost;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILocalizationManager _localizationManager;
        private readonly Language _language;
        private readonly Html _html;

        public Subtitle(IHttpClient httpClient, ILogger logger, IApplicationHost appHost, ILocalizationManager localizationManager,
            IJsonSerializer jsonSerializer)
        {
            _httpClient = httpClient;
            _logger = logger;
            _appHost = appHost;
            _jsonSerializer = jsonSerializer;
            _localizationManager = localizationManager;
            _language = new Language(localizationManager);
            _html = new Html();
        }

        public async Task<SubtitleResponse> Download(string id, CancellationToken cancellationToken)
        {
            var ids = id.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries);
            var url = ids[0].Replace("__", "/");
            var lang = ids[1];

            _logger?.Info($"Subf2m= Request for subtitle= {url}");

            var html = await _html.Get(Const.Domain, url);
            if (string.IsNullOrWhiteSpace(html))
                return new SubtitleResponse();

            var startIndex = html.IndexOf("<div class=\"download\">");
            var endIndex = html.IndexOf("</div>", startIndex);

            var downText = html.SubStr(startIndex, endIndex);
            startIndex = downText.IndexOf("<a href=\"");
            endIndex = downText.IndexOf("\"", startIndex + 10);

            var downloadLink = downText.SubStr(startIndex + 10, endIndex - 1);

            _logger?.Debug($"Subf2m= Downloading subtitle= {downloadLink}");

            var opts = BaseRequestOptions;
            opts.Url = $"{Const.Domain}/{downloadLink}";

            var ms = new MemoryStream();
            var fileExt = string.Empty;
            try
            {
                using var response = await _httpClient.GetResponse(opts).ConfigureAwait(false);
                _logger?.Info("Subf2m=" + response.ContentType);
                var contentType = response.ContentType.ToLower();
                if (!contentType.Contains("zip"))
                {
                    return new SubtitleResponse()
                    {
                        Stream = ms
                    };
                }

                var archive = new ZipArchive(response.Content);

                var item = (archive.Entries.Count > 1
                    ? archive.Entries.FirstOrDefault(a => a.FullName.ToLower().Contains("utf"))
                    : archive.Entries.First()) ?? archive.Entries.First();

                await item.Open().CopyToAsync(ms).ConfigureAwait(false);
                ms.Position = 0;

                fileExt = item.FullName.Split('.').LastOrDefault();

                if (string.IsNullOrWhiteSpace(fileExt))
                {
                    fileExt = "srt";
                }
            }
            catch (Exception e)
            {
                //
            }

            return new SubtitleResponse
            {
                Format = fileExt,
                Language = _language.Normalize(lang),
                Stream = ms
            };
        }

        public async Task<IEnumerable<RemoteSubtitleInfo>> Search(string title, int? year, string lang,
            VideoContentType contentType, string movieId, int season, int episode)
        {
            _logger?.Info(
                $"Subf2m= Request subtitle for '{title}', language={lang}, year={year}, movie Id={movieId}, Season={season}, Episode={episode}");

            var res = new List<RemoteSubtitleInfo>();
            try
            {
                switch (contentType)
                {
                    case VideoContentType.Movie:
                        var movie = new Movie(_httpClient, _logger, _jsonSerializer, _appHost, _localizationManager);
                        res = await movie.Search(title, year, lang, movieId);
                        break;
                    case VideoContentType.Episode:
                        var series = new Series(_httpClient, _logger, _jsonSerializer, _appHost, _localizationManager);
                        res = await series.Search(title, year, lang, movieId, season, episode);
                        break;

                    case VideoContentType.Audio:
                    default:
                        throw new ApplicationException($"His plugins not support {contentType}");
                }

                if (!res.Any())
                {
                    return res;
                }
            }
            catch (Exception e)
            {
                _logger?.Error(e.Message, e);
            }

            res.RemoveAll(l => string.IsNullOrWhiteSpace(l.Name));

            return res.OrderBy(s => s.Name);
        }


        private HttpRequestOptions BaseRequestOptions => new HttpRequestOptions
        {
            UserAgent = $"Emby/{_appHost?.ApplicationVersion}"
        };
    }
}