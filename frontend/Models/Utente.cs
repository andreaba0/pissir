namespace frontend.Models
{
    public class Utente
    {
        public string CodiceFiscale { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public string Role { get; set; }
        public string PartitaIva { get; set; }
        public string? ExpAccess { get; set; }
    }
}