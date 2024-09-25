using Newtonsoft.Json;

namespace frontend.Models
{
    public class UtentePeriodo : Utente
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("tax_code")]
        public string CodiceFiscale { get; set; }

        [JsonProperty("given_name")]
        public string Nome { get; set; }

        [JsonProperty("family_name")]
        public string Cognome { get; set; }

        [JsonProperty("company_vat_number")]
        public string PartitaIva { get; set; }

        [JsonProperty("date_start")]
        public string DataInizio {  get; set; }

        [JsonProperty("date_end")]
        public string DataFine { get; set; }        
    }
    
}