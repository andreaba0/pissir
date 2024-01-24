using Newtonsoft.Json;

namespace frontend.Models
{
    public class AcquaStimata
    {
        [JsonProperty("field_id")]
        public string CampoId { get; set; }

        [JsonProperty("total_estimated")]
        public float TotaleStimato { get; set; }

        [JsonProperty("total_remaining")]
        public float TotaleRimanente { get; set; }
    }
}
