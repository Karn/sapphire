using System.Collections.Generic;
using Windows.UI.Xaml;

namespace APIWrapper.Content.Model {
    public class Photo {
        public string caption { get; set; }
        public List<AltSize> alt_sizes { get; set; }
        public OriginalSize original_size { get; set; }

        public class AltSize {
            public int width { get; set; }
            public int height { get; set; }
            public int scaled_height {
                get {
                    return (int)((height / width) * (Window.Current.Bounds.Height));
                }
            }
            public string url { get; set; }
        }

        public class OriginalSize {
            public int width { get; set; }
            public int height { get; set; }
            public string url { get; set; }
        }
    }
}
