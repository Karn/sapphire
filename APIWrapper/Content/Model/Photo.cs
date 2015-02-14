using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace APIWrapper.Content.Model {
    public class Photo {
        public string caption { get; set; }
        public List<AltSize> alt_sizes { get; set; }
        public OriginalSize original_size { get; set; }


        public class AltSize {
            public int width { get; set; }
            public int height { get; set; }
            public BitmapImage scaled {
                get {
					return new BitmapImage() { UriSource = new System.Uri(url), DecodePixelHeight = (int)(height / width * (Window.Current.Bounds.Height - 12)), DecodePixelWidth = (int)(Window.Current.Bounds.Height - 12) };
                    //Debug.WriteLine(string.Format("Height: {0}, Width: {1}, Scaled Height: {2}", height, width, (int)((height / width) * (Window.Current.Bounds.Width - 20))));
                    //return (int)(height / width * (Window.Current.Bounds.Height - 12));
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
