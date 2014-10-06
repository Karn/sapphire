using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Content {
    public class Photo {
        public string caption { get; set; }
        public List<AltSize> alt_sizes { get; set; }
        public OriginalSize original_size { get; set; }

        public class AltSize {
            public int width { get; set; }
            public int height { get; set; }
            public string url { get; set; }
        }

        public class OriginalSize {
            public int width { get; set; }
            public int height { get; set; }
            public string url { get; set; }
        }
    }
}
