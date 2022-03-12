using System.Collections.Generic;

namespace Emby.Subtitle.SubF2M.Models
{
    public class FindMovie
    {
        public IEnumerable<TvEpisodeResult> tv_results { get; set; }
        public IEnumerable<TvEpisodeResult> tv_episode_results { get; set; }
    }
}