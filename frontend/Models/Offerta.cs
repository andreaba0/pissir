using Newtonsoft.Json;

namespace frontend.Models
{
    public class Offerta
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("amount")]
        public float Quantita { get; set; }

        [JsonProperty("price")]
        public float PrezzoLitro { get; set; }

        [JsonProperty("date")]
        public string DataAnnuncio { get; set; }
    }
}
