using System.Linq;
using Newtonsoft.Json;

namespace API.JsonLd
{
    public class Space : JsonLdBase
    {
        [JsonProperty(PropertyName = "modelId")]
        public int ModelId { get; set; } // the image identifier within the space

        [JsonProperty(PropertyName = "name")] 
        public string Name { get; set; }

        [JsonProperty(PropertyName = "defaultTags")]
        public string[] DefaultTags { get; set; }

        [JsonProperty(Order = 14, PropertyName = "defaultMaxUnauthorised")]
        public int DefaultMaxUnauthorised { get; set; }
    }

    public static class SpaceX
    {
        public static string ManifestTag = "dlcs:manifestSpace";
        
        /// <summary>
        /// Check 
        /// </summary>
        /// <param name="space"></param>
        /// <returns></returns>
        public static bool IsManifestSpace(this Space space) 
            => space.DefaultTags.Contains(ManifestTag);
    }
}