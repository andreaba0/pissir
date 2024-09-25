using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;

namespace frontend.Models
{
    public class Utente
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("given_name")]
        public string Nome { get; set; }

        [JsonProperty("family_name")]
        public string Cognome { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("tax_code")]
        public string CodiceFiscale { get; set; }

        [JsonProperty("role")]
        public string? Role { get; set; }

        [JsonProperty("company")]
        public string? PartitaIva { get; set; }
    }

}