using Newtonsoft.Json;

namespace frontend.Models
{
    public class Azienda
    {
        [JsonProperty("vat_number")]
        public string PartitaIva { get; set; }

        [JsonProperty("company_name")]
        public string Nome { get; set; }

        [JsonProperty("working_address")]
        public string Indirizzo { get; set; }

        [JsonProperty("working_phone_number")]
        public string Telefono { get; set; }

        [JsonProperty("working_email_address")]
        public string Email { get; set; }

        [JsonProperty("industry_sector")]
        public string Categoria { get; set; }
    }
}
