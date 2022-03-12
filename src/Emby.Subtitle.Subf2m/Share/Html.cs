using System.Net.Http;
using System.Threading.Tasks;

namespace Emby.Subtitle.SubF2M.Share
{
    public class Html
    {
        public async Task<string> Get(string domain, string path)
        {
            var html = await Tools.RequestUrl(domain, path, HttpMethod.Get).ConfigureAwait(false);

            var headIndex = html.IndexOf("<head>");
            var headEnd = html.IndexOf("</head>", headIndex + 1);
            var endHeadBlock = headEnd - headIndex + 7;
            html = html.Remove(headIndex, endHeadBlock);

            var scIndex = html.IndexOf("<script");
            while (scIndex >= 0)
            {
                var scEnd = html.IndexOf("</script>", scIndex + 1);
                var end = scEnd - scIndex + 9;
                html = html.Remove(scIndex, end);
                scIndex = html.IndexOf("<script");
            }

            scIndex = html.IndexOf("&#");
            while (scIndex >= 0)
            {
                var scEnd = html.IndexOf(";", scIndex + 1);
                var end = scEnd - scIndex + 1;
                var word = html.Substring(scIndex, end);
                html = html.Replace(word, System.Net.WebUtility.HtmlDecode(word));
                scIndex = html.IndexOf("&#");
            }

            html = html.Replace("&nbsp;", "");
            html = html.Replace("&amp;", "Xamp;");
            html = html.Replace("&", "&amp;");
            html = html.Replace("Xamp;", "&amp;");
            html = html.Replace("--->", "---");
            html = html.Replace("<---", "---");
            html = html.Replace("<--", "--");
            html = html.Replace("Xamp;", "&amp;");
            html = html.Replace("<!DOCTYPE html>", "");
            return html;
        }

    }
}