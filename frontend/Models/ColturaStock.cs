using Newtonsoft.Json;

namespace frontend.Models
{
    public class ColturaStock
    {
        [JsonProperty("field_id")]
        public string Id { get; set; }

        [JsonProperty("limit")]
        public float AmountRemaining { get; set; }
    }
}
