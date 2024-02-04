using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse
{
    public class URLEntity
    {
        public string URL { get; set; }
        public string HTML { get; set; }
        public string Text { get; set; }
        public List<string> Links { get; set; }

    }
}
