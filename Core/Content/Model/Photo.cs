using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Core.Content.Model {
	public class Photo {
		public string caption { get; set; }
		public List<AltSize> alt_sizes { get; set; }
		public int ColSpan { get; set; }
		public int RowSpan { get; set; }
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
