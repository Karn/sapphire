using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Content {
    public class Tag {
        public string tag { get; set; }
        public object thumb_url { get; set; }
        public bool featured { get; set; }
        public string url { get; set; }
    }
}
