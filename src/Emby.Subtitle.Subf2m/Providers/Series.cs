﻿using Emby.Subtitle.SubF2M.Extensions;
using Emby.Subtitle.SubF2M.Models;
using Emby.Subtitle.SubF2M.Share;
using MediaBrowser.Common;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Model.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace Emby.Subtitle.SubF2M.Providers
{
    public class Series
    {
        private readonly string[] _seasonNumbers =
            { "", "First", "Second", "Third", "Fourth", "Fifth", "Sixth", "Seventh", "Eighth", "Ninth" };

        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly IApplicationHost _appHost;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly Language _language;
        private readonly Html _html;

        public Series(IHttpClient httpClient, ILogger logger, IJsonSerializer jsonSerializer, IApplicationHost appHost,
            ILocalizationManager localizationManager)
        {
            _httpClient = httpClient;
            _logger = logger;
            _appHost = appHost;
            _jsonSerializer = jsonSerializer;
            _language = new Language(localizationManager);
            _html = new Html(httpClient, appHost);
        }

        public async Task<List<RemoteSubtitleInfo>> Search(string lang, string movieId, int season, int episode,
            CancellationToken cancellationToken)
        {
            var mDb = new MovieDb(_jsonSerializer, _httpClient, _appHost);
            var info = await mDb.GetTvInfo(movieId, cancellationToken);

            if (info == null)
            {
                return new List<RemoteSubtitleInfo>();
            }

            #region Search TV Shows

            var html = await SearchSubF2M(info, season, lang, cancellationToken);

            #endregion

            #region Extract subtitle links

            var subtitles = ExtractSubtitles(html, season, episode, lang);

            #endregion

            return subtitles;
        }

        #region Private Methods

        private async Task<string> SearchSubF2M(TvInformation info, int season, string lang,
            CancellationToken cancellationToken)
        {
            var title = info.Name;

            _logger?.Debug($"Subf2m= Searching for site search \"{title}\"");

            var url = string.Format(Const.SearchUrl,
                HttpUtility.UrlEncode($"{title} - {_seasonNumbers[season]} Season"));
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

            var ulNode = xNode.SelectSingleNode("ul");
            var sItems = ulNode?.SelectNodes(".//a");
            if (sItems == null)
            {
                return string.Empty;
            }

            foreach (XmlNode item in sItems)
            {
                if (!item.InnerText.StartsWith($"{title} - {_seasonNumbers[season]} Season"))
                {
                    continue;
                }

                var link = item.Attributes?["href"].Value;
                link += $"/{_language.Map(lang)}";
                html = await _html.Get(Const.Domain + link, cancellationToken);
                break;
            }

            return html;
        }

        private static List<RemoteSubtitleInfo> ExtractSubtitles(string html, int season, int episode, string lang)
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

            var eTitle = $"S{season.ToString().PadLeft(2, '0')}E{episode.ToString().PadLeft(2, '0')}";
            foreach (XmlElement node in repeater)
            {
                var nameList = node.SelectNodes(".//li");
                var name = nameList?.Cast<XmlElement>()
                    .Aggregate(string.Empty, (current, nItem) => current + (nItem.InnerText + "<br/>"));

                if (string.IsNullOrWhiteSpace(name) || !name.Contains(eTitle))
                {
                    continue;
                }

                var id = Subtitle.CreateSubtitleId(
                    node.SelectSingleNode(".//a[@class='download icon-download']")?.Attributes?["href"].Value,
                    lang);

                var author = node.SelectSingleNode(".//p")?.InnerText.RemoveExtraCharacter();
                var provider = node.SelectSingleNode(".//div[@class='vertical-middle']/b/a")?.InnerText
                    .RemoveExtraCharacter();

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