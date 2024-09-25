using Newtonsoft.Json;

namespace frontend.Models
{
    public class Azienda
    {
        [JsonProperty("vat_number")]
        public string PartitaIva { get; set; }

        [JsonProperty("name")]
        public string Nome { get; set; }

        [JsonProperty("address")]
        public string Indirizzo { get; set; }

        [JsonProperty("phone")]
        public string Telefono { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("industry_sector")]
        public string Categoria { get; set; }
    }
}
