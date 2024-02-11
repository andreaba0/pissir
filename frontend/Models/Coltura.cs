using Newtonsoft.Json;

namespace frontend.Models
{
    public class Coltura
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("company_vat_number")]
        public string? PartitaIva { get; set; }

        [JsonProperty("square_meters")]
        public float MetriQuadrati { get; set; }

        [JsonProperty("crop_type")]
        public string TipoColtura { get; set; }

        [JsonProperty("irrigation_type")]
        public string TipoIrrigazione { get; set; }
    }
}
