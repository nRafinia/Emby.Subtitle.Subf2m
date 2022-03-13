using Emby.Subtitle.SubF2M.Extensions;
using Emby.Subtitle.SubF2M.Share;
using MediaBrowser.Common;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Model.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace Emby.Subtitle.SubF2M.Providers
{
    public class Movie
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly IApplicationHost _appHost;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly Language _language;
        private readonly Html _html;

        public Movie(IHttpClient httpClient, ILogger logger, IJsonSerializer jsonSerializer, IApplicationHost appHost,
            ILocalizationManager localizationManager)
        {
            _httpClient = httpClient;
            _logger = logger;
            _appHost = appHost;
            _jsonSerializer = jsonSerializer;
            _language = new Language(localizationManager);
            _html = new Html(httpClient, appHost);
        }

        public async Task<List<RemoteSubtitleInfo>> Search(string title, int? year, string lang, string movieId,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(movieId))
            {
                var mDb = new MovieDb(_jsonSerializer, _httpClient, _appHost);
                var info = await mDb.GetMovieInfo(movieId, cancellationToken);

                if (info != null)
                {
                    year = info.release_date.Year;
                    title = info.Title;
                    _logger?.Info($"Subf2m= Original movie title=\"{info.Title}\", year={info.release_date.Year}");
                }
            }

            #region Search Subf2m

            _logger?.Debug($"Subf2m= Searching for site search \"{title}\"");
            var html = await SearchSubF2M(title, year, lang, cancellationToken);

            if (string.IsNullOrWhiteSpace(html))
            {
                return new List<RemoteSubtitleInfo>();
            }

            #endregion

            #region Extract subtitle links

            var subtitles = ExtractSubtitles(html, lang);

            #endregion

            return subtitles;
        }

        #region Private Methods

        private async Task<string> SearchSubF2M(string title, int? year, string lang, CancellationToken cancellationToken)
        {
            var url = string.Format(Const.SearchUrl, HttpUtility.UrlEncode(title));
            var html = await _html.Get(Const.Domain + url, cancellationToken);

            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            html = html.Replace("</div></div></body>", "</div></body>");

            var xml = new XmlDocument();
            xml.LoadXml(Const.XmlTag + html);

            var xNode = xml.SelectSingleNode("//div[@class='search-result']");
            if (xNode == null)
            {
                return string.Empty;
            }

            var ex = xNode.SelectSingleNode("h2[@class='exact']")
                     ?? xNode.SelectSingleNode("h2[@class='close']")
                     ?? xNode.SelectSingleNode("h2[@class='popular']");

            if (ex == null)
            {
                return string.Empty;
            }

            xNode = xNode.SelectSingleNode("ul");
            var sItems = xNode?.SelectNodes(".//a");

            if (sItems == null)
            {
                return string.Empty;
            }

            foreach (XmlNode item in sItems)
            {
                if (item == null)
                {
                    continue;
                }

                var sYear = item.InnerText.Split('(', ')')[1];
                if ((year ?? 0) != Convert.ToInt16(sYear))
                {
                    continue;
                }

                var link = $"{item.Attributes?["href"].Value}/{_language.Map(lang)}";
                html = await _html.Get(Const.Domain + link, cancellationToken);
                break;
            }

            return html;
        }

        private static List<RemoteSubtitleInfo> ExtractSubtitles(string html, string lang)
        {
            html = html.Replace("</div></div></body>", "</div></body>");

            var res = new List<RemoteSubtitleInfo>();

            var xml = new XmlDocument();
            xml.LoadXml(Const.XmlTag + html);

            var repeater = xml.SelectNodes("//li[@class='item']");

            if (repeater == null)
            {
                return res;
            }

            foreach (XmlElement node in repeater)
            {
                var nameList = node.SelectNodes(".//li");

                if (nameList == null)
                {
                    continue;
                }

                var name = nameList.Cast<XmlElement>()
                    .Aggregate(string.Empty, (current, nItem) => current + (nItem.InnerText + "<br/>"));

                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var id = Subtitle.CreateSubtitleId(
                    node.SelectSingleNode(".//a[@class='download icon-download']")?.Attributes?["href"].Value,
                    lang);

                var author = node.SelectSingleNode(".//p")?.InnerText.RemoveExtraCharacter();
                var provider =
                    node.SelectSingleNode(".//div[@class='vertical-middle']/b/a")?.InnerText.RemoveExtraCharacter();

                var item = new RemoteSubtitleInfo
                {
                    Id = id,
                    Name = $"{provider} ({author})",
                    Author = author,
                    ProviderName = Const.PluginName,
                    Comment = name,
                    Format = "srt"
                };
                res.Add(item);
            }

            return res;
        }

        #endregion
    }
}