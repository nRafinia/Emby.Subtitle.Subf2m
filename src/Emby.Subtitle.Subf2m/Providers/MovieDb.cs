using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Emby.Subtitle.Subf2m.Models;
using MediaBrowser.Common;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Serialization;
using MovieFinder.Models;

namespace Emby.Subtitle.Subf2m.Providers
{
    public class MovieDb
    {
        private const string token = "{TOKEN}";// Get https://www.themoviedb.org/ API token
        private readonly string _movieUrl = "https://api.themoviedb.org/3/movie/{0}?api_key={1}";
        private readonly string _tvUrl = "https://api.themoviedb.org/3/tv/{0}?api_key={1}";
        private readonly string _searchMovie = "https://api.themoviedb.org/3/find/{0}?api_key={1}&external_source={2}";

        private readonly IJsonSerializer _jsonSerializer;
        private readonly IHttpClient _httpClient;
        private readonly IApplicationHost _appHost;
        public MovieDb(IJsonSerializer jsonSerializer, IHttpClient httpClient, IApplicationHost appHost)
        {
            _jsonSerializer = jsonSerializer;
            _httpClient = httpClient;
            _appHost = appHost;
        }

        public async Task<MovieInformation> GetMovieInfo(string id)
        {
            var opts = BaseRequestOptions;
            opts.Url = string.Format(_movieUrl, id, token);

#if DEBUG
            var searchResults = await Tools.RequestUrl<MovieInformation>(opts.Url, "", HttpMethod.Get);
            return searchResults;
#else
            using var response = await _httpClient.GetResponse(opts);/*.ConfigureAwait(false)*/
            
                if (response.ContentLength < 0)
                    return null;

                var searchResults = _jsonSerializer.DeserializeFromStream<MovieInformation>(response.Content);

                return searchResults;
            
#endif
        }

        public async Task<FindMovie> SearchMovie(string id)
        {
            var opts = BaseRequestOptions;
            var type = id.StartsWith("tt") ? MovieSourceType.imdb_id : MovieSourceType.tvdb_id;
            opts.Url = string.Format(_searchMovie, id, token, type.ToString());

#if DEBUG
            var searchResults = await Tools.RequestUrl<FindMovie>(opts.Url, "", HttpMethod.Get);
#else
            using var response = await _httpClient.GetResponse(opts).ConfigureAwait(false);
            if (response.ContentLength < 0)
                return null;

            var searchResults = _jsonSerializer.DeserializeFromStream<FindMovie>(response.Content);
#endif
            return searchResults;
        }

        public async Task<TvInformation> GetTvInfo(string id)
        {
            var movie = await SearchMovie(id);

            if ((movie?.tv_results == null || !movie.tv_results.Any()) &&
                (movie?.tv_episode_results == null || !movie.tv_episode_results.Any()))
            {
                return null;
            }

            var opts = BaseRequestOptions;
            opts.Url = string.Format(
                _tvUrl, 
                movie.tv_results?.FirstOrDefault()?.id ??
                movie.tv_episode_results.First().show_id, 
                token);

#if DEBUG
            var searchResults = await Tools.RequestUrl<TvInformation>(opts.Url, "", HttpMethod.Get);
#else
            using var response = await _httpClient.GetResponse(opts).ConfigureAwait(false);
            if (response.ContentLength < 0)
                return null;

            var searchResults = _jsonSerializer.DeserializeFromStream<TvInformation>(response.Content);
#endif
            return searchResults;
        }


        private HttpRequestOptions BaseRequestOptions => new HttpRequestOptions
        {
            UserAgent = $"Emby/{_appHost?.ApplicationVersion}"
        };

    }
}
