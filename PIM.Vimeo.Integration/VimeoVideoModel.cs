using System.Collections.Generic;
using Newtonsoft.Json;

namespace PIM.Vimeo.Integration
{
    public class VimeoVideoModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("pictures")]
        public VimeoVideoPicture Pictures { get; set; }
    }

    public class VimeoVideoPicture
    {
        [JsonProperty("sizes")]
        public List<VimeoVideoPictureSize> Sizes { get; set; }
    }

    public class VimeoVideoPictureSize
    {
        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("link_with_play_button")]
        public string LinkPlayButton { get; set; }
    }

    public class VimeoTest
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("token_is_authenticated")]
        public bool IsAuthenticated { get; set; }
    }
}
