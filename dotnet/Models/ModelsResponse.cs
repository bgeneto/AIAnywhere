using System.Collections.Generic;
using Newtonsoft.Json;

namespace AIAnywhere.Models
{
    public class ModelsResponse
    {
        [JsonProperty("data")]
        public List<ModelInfo> Data { get; set; } = new List<ModelInfo>();

        [JsonProperty("object")]
        public string Object { get; set; } = "";
    }

    public class ModelInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; } = "";

        [JsonProperty("object")]
        public string Object { get; set; } = "";

        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("owned_by")]
        public string OwnedBy { get; set; } = "";
    }
}
