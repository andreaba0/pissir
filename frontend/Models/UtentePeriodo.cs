using Newtonsoft.Json;

namespace frontend.Models
{
    public class UtentePeriodo : Utente
    {
        [JsonProperty("date_start")]
        public string DataInizio {  get; set; }

        [JsonProperty("date_end")]
        public string DataFine { get; set; }
    }
}