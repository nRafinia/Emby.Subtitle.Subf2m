using MediaBrowser.Common.Net;
using MediaBrowser.Model.Serialization;
using System.Threading.Tasks;

namespace Emby.Subtitle.SubF2M.Extensions
{
    public static class HttpClientExt
    {
        public static async Task<T> GetResponse<T>(this IHttpClient httpClient, HttpRequestOptions options,
            IJsonSerializer jsonSerializer)
        {
            using var response = await httpClient.GetResponse(options);

            return response.ContentLength <= 0
                ? default
                : jsonSerializer.DeserializeFromStream<T>(response.Content);
        }
    }
}