using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSliderWPF.Models
{
    public class Settings
    {
        public Settings() 
        {
            this.rss = new RSS();
        }
        public RSS rss { get; set; }
    }
}
