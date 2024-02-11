using Newtonsoft.Json;

namespace frontend.Models
{
    public class LimiteAcquistoAzienda
    {
        [JsonProperty("company_vat_number")]
        public string PartitaIva { get; set; }

        [JsonProperty("limit")]
        public float LimiteAcquistoAziendale { get; set; }

        [JsonProperty("start_date")]
        public string DataInizio { get; set; }

        [JsonProperty("end_date")]
        public string DataFine { get; set; }
    }
}
