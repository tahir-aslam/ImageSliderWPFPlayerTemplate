using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSliderWPF
{
    public class RSSItem
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string EncodedContent { get; set; }

        public string CompleteContent { get; set; }

        public string HtmlContent { get; set; }

        public string Link { get; set; }

        public string DetailUrl { get; set; }

        public DateTime PubDate { get; set; }

        public string Category { get; set; }
    }

}
