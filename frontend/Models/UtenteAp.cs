using Newtonsoft.Json;

namespace frontend.Models
{
    public class UtenteAp
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("company_vat_number")]
        public string? PartitaIva { get; set; }

        [JsonProperty("given_name")]
        public string? Nome { get; set; }

        [JsonProperty("family_name")]
        public string? Cognome { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("tax_code")]
        public string? CodiceFiscale { get; set; }

        [JsonProperty("company_category")]
        public string? TipoAzienda { get; set; }
    }
}
