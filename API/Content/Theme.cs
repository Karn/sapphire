using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Content {
    public class Theme {
        public string avatar_shape { get; set; }
        public string background_color { get; set; }
        public string body_font { get; set; }
        public string header_bounds { get; set; }
        public string header_image { get; set; }
        public string header_image_focused { get; set; }
        public string header_image_scaled { get; set; }
        public bool header_stretch { get; set; }
        public string link_color { get; set; }
        public bool show_avatar { get; set; }
        public bool show_description { get; set; }
        public bool show_header_image { get; set; }
        public bool show_title { get; set; }
        public string title_color { get; set; }
        public string title_font { get; set; }
        public string title_font_weight { get; set; }
        public int? header_full_width { get; set; }
        public int? header_full_height { get; set; }
        public int? header_focus_width { get; set; }
        public int? header_focus_height { get; set; }
    }
}
