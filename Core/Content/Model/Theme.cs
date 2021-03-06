﻿using Newtonsoft.Json;

namespace Core.Content.Model {
    public class Theme {
        [JsonProperty("header_image_focused")]
        public string HeaderImage { get; set; }

        [JsonProperty("header_image_scaled")]
        public string ScaledHeaderImage { get; set; }
    }
}
