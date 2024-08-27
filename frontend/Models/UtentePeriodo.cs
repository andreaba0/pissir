using Newtonsoft.Json;

namespace frontend.Models
{
    public class UtentePeriodo : Utente
    {
        [JsonProperty("date_start")]
        public string DataInizio {  get; set; }

        [JsonProperty("date_end")]
        public string DataFine { get; set; }

        [JsonProperty("acl_id")]
        public string Id { get; set; }

        [JsonProperty("first_name")]
        public string Nome { get; set; }

        [JsonProperty("last_name")]
        public string Cognome { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("company_vat_number")]
        public string PartitaIva { get; set; }

        [JsonProperty("tax_code")]
        public string CodiceFiscale {  get; set; }
    }
    
}