using Newtonsoft.Json;

namespace frontend.Models
{
    public class SensoreLog
    {
        [JsonProperty("object_id")]
        public string Id { get; set; }

        [JsonProperty("field_id")]
        public string IdCampo { get; set; }

        [JsonProperty("type")]
        public string Tipo { get; set; }

        [JsonProperty("time")]
        public required string Time { get; set; }

        [JsonProperty("amount")]
        public required float Misura { get; set; }
    }
}
