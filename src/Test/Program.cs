using System;
using System.Threading.Tasks;
using Emby.Subtitle.Subf2m.Providers;
using MediaBrowser.Controller.Providers;

namespace Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var txt = "The Old Guard";
            var lang = "per";
            var searchResult = await new Subf2mSubtitleProvider(null, null, null, null, null)
                .Search(txt, 2020, lang, VideoContentType.Movie, "547016", 0, 0);

            Console.WriteLine("Result:");
            foreach (var item in searchResult)
            {
                Console.WriteLine($"{item.Name} - {item.Id}");
            }

            /*await new Subf2mSubtitleProvider(null, null, null, null).GetSubtitles(
                "__subtitles__bright__farsi_persian__1922088___per", CancellationToken.None);*/


            //var text = File.ReadAllText(file, Encoding.GetEncoding(codePage));

            Console.ReadKey();
        }
    }
}
