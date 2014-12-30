using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;

namespace APIWrapper.Content.Model {
    public class Photo {
        public string caption { get; set; }
        public List<AltSize> alt_sizes { get; set; }
        public OriginalSize original_size { get; set; }

        public AltSize path_to_low_res_pic {
            get {
                return alt_sizes.First();
            }
        }

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
