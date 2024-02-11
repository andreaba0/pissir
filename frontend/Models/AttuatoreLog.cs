using Newtonsoft.Json;

namespace frontend.Models
{
    public class AttuatoreLog
    {
        [JsonProperty("object_id")]
        public string Id { get; set; }

        [JsonProperty("field_id")]
        public string IdCampo { get; set; }

        [JsonProperty("time")]
        public required string Time { get; set; }

        [JsonProperty("active")]
        public required bool Attivo { get; set; }
    }
}
