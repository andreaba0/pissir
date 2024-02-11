using Newtonsoft.Json;

namespace frontend.Models
{
    public class OrdineAcquisto
    {
        [JsonProperty("offer_id")]
        public string IdOfferta { get; set; }

        [JsonProperty("company_vat_number")]
        public string PartitaIva {  get; set; }

        [JsonProperty("field_id")]
        public string IdCampo { get; set; }

        [JsonProperty("amount")]
        public float Quantita { get; set; }
    }
}
