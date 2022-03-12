using Emby.Subtitle.SubF2M.Share;
using MediaBrowser.Common;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Model.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Emby.Subtitle.SubF2M
{
    public class SubF2MSubtitleProvider : ISubtitleProvider, IHasOrder
    {
        public string Name => Const.PluginName;

        public IEnumerable<VideoContentType> SupportedMediaTypes =>
            new List<VideoContentType>()
            {
                VideoContentType.Movie,
                VideoContentType.Episode
            };

        public int Order => 0;

        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly IApplicationHost _appHost;
        private readonly ILocalizationManager _localizationManager;
        private readonly IJsonSerializer _jsonSerializer;

        public SubF2MSubtitleProvider(IHttpClient httpClient, ILogger logger, IApplicationHost appHost,
            ILocalizationManager localizationManager, IJsonSerializer jsonSerializer)
        {
            _httpClient = httpClient;
            _logger = logger;
            _appHost = appHost;
            _localizationManager = localizationManager;
            _jsonSerializer = jsonSerializer;
        }

        public async Task<IEnumerable<RemoteSubtitleInfo>> Search(SubtitleSearchRequest request,
            CancellationToken cancellationToken)
        {
            var prov = request.ProviderIds?.FirstOrDefault(p =>
                p.Key.ToLower() == "imdb" || p.Key.ToLower() == "tmdb" || p.Key.ToLower() == "tvdb");

            if (prov == null)
            {
                return new List<RemoteSubtitleInfo>();
            }

            if (request.ContentType == VideoContentType.Episode &&
                (request.ParentIndexNumber == null || request.IndexNumber == null))
            {
                return new List<RemoteSubtitleInfo>();
            }

            var title = request.ContentType == VideoContentType.Movie
                ? request.Name
                : request.SeriesName;

            var subtitle = new Providers.Subtitle(_httpClient, _logger, _appHost, _localizationManager, _jsonSerializer);
            var subtitleResult = await subtitle.Search(
                title,
                request.ProductionYear,
                request.Language,
                request.ContentType,
                prov.Value.Value,
                request.ParentIndexNumber ?? 0,
                request.IndexNumber ?? 0,
                cancellationToken);

            var subtitles = subtitleResult as RemoteSubtitleInfo[] ?? subtitleResult.ToArray();

            _logger?.Debug($"Subf2m= result found={subtitles?.Count()}");
            return subtitles;
        }

        public Task<SubtitleResponse> GetSubtitles(string id, CancellationToken cancellationToken)
        {
            var subtitle = new Providers.Subtitle(_httpClient, _logger, _appHost, _localizationManager, _jsonSerializer);
            return subtitle.Download(id, cancellationToken);
        }

    }
}