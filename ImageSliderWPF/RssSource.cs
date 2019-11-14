using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSliderWPF
{
    public class RSSSource
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Link { get; set; }

        /// <summary>
        /// The publication date for the content in the channel. 
        /// </summary>
        public DateTime PubDate { get; set; }

        /// <summary>
        /// The last time the content of the channel changed.
        /// </summary>
        public DateTime LastBuildDate { get; set; }

        /// <summary>
        /// It's a number of minutes that indicates how long a 
        /// channel can be cached before refreshing from the source
        /// </summary>
        public int TimeToLive { get; set; }

        public string Copyright { get; set; }

        /// <summary>
        /// The language the channel is written in.
        /// eg: en-us
        /// </summary>
        public string Language { get; set; }
    }
}
