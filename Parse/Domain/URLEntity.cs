using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.Domain
{
    public class URLEntity
    {
        public string URL { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public List<string> Links { get; set; }

    }
}
