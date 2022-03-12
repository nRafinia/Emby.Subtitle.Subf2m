using Emby.Subtitle.SubF2M;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //var txt = "The Old Guard";
            var txt = "wandavision";
            var lang = "per";
            /*var searchResult = await new Subf2mSubtitleProvider(null, null, null, null, null)
                .Search(txt, 2020, lang, VideoContentType.Movie, "547016", 0, 0);*/

            /*var searchResult = await new Subf2mSubtitleProvider(null, null, null, null, null)
                .Search(txt, 2020, lang, VideoContentType.Episode, "tt9140560", 1, 1);

            Console.WriteLine("Result:");
            foreach (var item in searchResult)
            {
                Console.WriteLine($"{item.Name} - {item.Id}");
            }*/

            //__subtitles__wandavision__farsi_persian__2373648___per
            await new SubF2MSubtitleProvider(null, null, null, null, null)
                .GetSubtitles("__subtitles__wandavision__farsi_persian__2373648___per", CancellationToken.None);


            //var text = File.ReadAllText(file, Encoding.GetEncoding(codePage));

            Console.ReadKey();
        }
    }
}
