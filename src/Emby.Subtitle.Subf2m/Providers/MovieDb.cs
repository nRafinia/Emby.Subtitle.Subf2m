using Emby.Subtitle.SubF2M.Extensions;
using Emby.Subtitle.SubF2M.Models;
using Emby.Subtitle.SubF2M.Share;
using MediaBrowser.Common;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Serialization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Emby.Subtitle.SubF2M.Providers
{
    public class MovieDb
    {
        private const string Token = "{token}"; // Get https://www.themoviedb.org/ API token
        private const string MovieUrl = "https://api.themoviedb.org/3/movie/{0}?api_key={1}";
        private const string TvUrl = "https://api.themoviedb.org/3/tv/{0}?api_key={1}";
        private const string SearchMovieUrl = "https://api.themoviedb.org/3/find/{0}?api_key={1}&external_source={2}";

        private readonly IJsonSerializer _jsonSerializer;
        private readonly IHttpClient _httpClient;
        private readonly Html _html;

        public MovieDb(IJsonSerializer jsonSerializer, IHttpClient httpClient, IApplicationHost appHost)
        {
            _jsonSerializer = jsonSerializer;
            _httpClient = httpClient;
            _html = new Html(httpClient, appHost);
        }

        public Task<MovieInformation> GetMovieInfo(string id, CancellationToken cancellationToken)
        {
            var opts = _html.BaseRequestOptions(
                string.Format(MovieUrl, id, Token),
                cancellationToken);

            return _httpClient.GetResponse<MovieInformation>(opts, _jsonSerializer);
        }

        public async Task<TvInformation> GetTvInfo(string id, CancellationToken cancellationToken)
        {
            var movie = await SearchMovie(id, cancellationToken);

            if ((movie?.tv_results == null || !movie.tv_results.Any()) &&
                (movie?.tv_episode_results == null || !movie.tv_episode_results.Any()))
            {
                return null;
            }

            var url = string.Format(
                TvUrl,
                movie.tv_results?.FirstOrDefault()?.id ??
                movie.tv_episode_results.First().show_id,
                Token);

            var opts = _html.BaseRequestOptions(url, cancellationToken);

            var searchResults = await _httpClient.GetResponse<TvInformation>(opts, _jsonSerializer);
            return searchResults;
        }

        private Task<FindMovie> SearchMovie(string id, CancellationToken cancellationToken)
        {
            var type = id.StartsWith("tt") ? MovieSourceType.imdb_id : MovieSourceType.tvdb_id;
            var opts = _html.BaseRequestOptions(
                string.Format(SearchMovieUrl, id, Token, type.ToString()),
                cancellationToken);

            return _httpClient.GetResponse<FindMovie>(opts, _jsonSerializer);
        }
    }
}