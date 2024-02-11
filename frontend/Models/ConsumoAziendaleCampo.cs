using Newtonsoft.Json;

namespace frontend.Models
{
    public class ConsumoAziendaleCampo
    {
        [JsonProperty("company_vat_number")]
        public string? PartitaIva { get; set; }

        [JsonProperty("field_id")]
        public string Campo { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("amount_used")]
        public float LitriConsumati { get; set; }

        [JsonProperty("amount_ordered")]
        public float QuantitaPrenotata { get; set; }
    }
}
