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

        public Subtitle(IHttpClient httpClient, ILogger logger, IApplicationHost appHost,
            ILocalizationManager localizationManager, IJsonSerializer jsonSerializer)
        {
            _httpClient = httpClient;
            _logger = logger;
            _appHost = appHost;
            _jsonSerializer = jsonSerializer;
            _localizationManager = localizationManager;
            _language = new Language(localizationManager);
            _html = new Html(httpClient, appHost);
        }

        public async Task<IEnumerable<RemoteSubtitleInfo>> Search(string title, int? year, string lang,
            VideoContentType contentType, string movieId, int season, int episode, CancellationToken cancellationToken)
        {
            _logger?.Info(
                $"Subf2m= Request subtitle for '{title}', language={lang}, year={year}, movie Id={movieId}, Season={season}, Episode={episode}");

            List<RemoteSubtitleInfo> subtitles = null;
            try
            {
                switch (contentType)
                {
                    case VideoContentType.Movie:
                        var movie = new Movie(_httpClient, _logger, _jsonSerializer, _appHost, _localizationManager);
                        subtitles = await movie.Search(title, year, lang, movieId, cancellationToken);
                        break;

                    case VideoContentType.Episode:
                        var series = new Series(_httpClient, _logger, _jsonSerializer, _appHost, _localizationManager);
                        subtitles = await series.Search(lang, movieId, season, episode, cancellationToken);
                        break;

                    default:
                        throw new ApplicationException($"His plugins not support {contentType}");
                }
            }
            catch (Exception e)
            {
                _logger?.Error(e.Message, e);
            }

            if (subtitles == null || !subtitles.Any())
            {
                return new List<RemoteSubtitleInfo>();
            }

            subtitles.RemoveAll(l => string.IsNullOrWhiteSpace(l.Name));
            return subtitles.OrderBy(s => s.Name);
        }

        public async Task<SubtitleResponse> Download(string id, CancellationToken cancellationToken)
        {
            var (url, lang) = ExtractSubtitleParts(id);

            _logger?.Info($"Subf2m= Request for subtitle= {url}");

            var html = await _html.Get(Const.Domain + url, cancellationToken);
            if (string.IsNullOrWhiteSpace(html))
            {
                return new SubtitleResponse();
            }

            var downloadLink = ExtractDownloadLink(html);

            _logger?.Debug($"Subf2m= Downloading subtitle= {downloadLink}");

            var subtitle = await DownloadSubtitle($"{Const.Domain}/{downloadLink}", lang, cancellationToken);

            return new SubtitleResponse()
            {
                Format = subtitle.Extension,
                Language = subtitle.Language,
                Stream = subtitle.content
            };
        }

        public static string CreateSubtitleId(string url, string lang)
        {
            return $"{url}___{lang}".Replace("/", "__");
        }

        #region Private methods

        private static (string Url, string Lang) ExtractSubtitleParts(string id)
        {
            var ids = id.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries);
            var url = ids[0].Replace("__", "/");
            var lang = ids[1];

            return (url, lang);

        }

        private static string ExtractDownloadLink(string html)
        {
            var startIndex = html.IndexOf("<div class=\"download\">", StringComparison.Ordinal);
            var endIndex = html.IndexOf("</div>", startIndex, StringComparison.Ordinal);

            var downText = html.SubStr(startIndex, endIndex);
            startIndex = downText.IndexOf("<a href=\"", StringComparison.Ordinal);
            endIndex = downText.IndexOf("\"", startIndex + 10, StringComparison.Ordinal);

            var downloadLink = downText.SubStr(startIndex + 10, endIndex - 1);
            return downloadLink;
        }

        private async Task<(string Extension, string Language, Stream content)> DownloadSubtitle(string url, string lang, CancellationToken cancellationToken)
        {
            var opts = _html.BaseRequestOptions(url, cancellationToken);

            try
            {
                using var response = await _httpClient.GetResponse(opts);
                _logger?.Info("Subf2m=" + response.ContentType);

                var contentType = response.ContentType.ToLower();
                if (!contentType.Contains("zip"))
                {
                    return (string.Empty, string.Empty, new MemoryStream());
                }

                var (stream, fileName) = await GetSubtitleFromZip(response.Content, cancellationToken);

                var fileExt = Path.GetExtension(fileName);
                if (string.IsNullOrWhiteSpace(fileExt))
                {
                    fileExt = "srt";
                }

                return (fileExt, _language.Normalize(lang), stream);
            }
            catch (Exception e)
            {
                _logger?.Error(e.Message);
            }

            return (string.Empty, string.Empty, new MemoryStream());
        }

        private static async Task<(MemoryStream Stream, string FileName)> GetSubtitleFromZip(Stream zipStream, CancellationToken cancellationToken)
        {
            var archive = new ZipArchive(zipStream);

            var item = (archive.Entries.Count > 1
                ? archive.Entries.FirstOrDefault(a => a.FullName.ToLower().Contains("utf"))
                : archive.Entries.First()) ?? archive.Entries.First();

            await using var ms = new MemoryStream();
            await item.Open().CopyToAsync(ms, cancellationToken);
            ms.Position = 0;

            return (ms, item.FullName);
        }

        #endregion

    }
}