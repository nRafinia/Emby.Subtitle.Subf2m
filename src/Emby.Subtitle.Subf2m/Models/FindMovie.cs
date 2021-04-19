using System.Collections.Generic;

namespace Emby.Subtitle.Subf2m.Models
{
    public class FindMovie
    {
        public IEnumerable<TvEpisodeResult> tv_episode_results { get; set; }
    }
}