using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace frontend.Pages.auth
{
    public class SignToFarmModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public SignToFarmModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CodiceFiscale { get; set; }
        public string NomeUtente { get; set; }
        public string CognomeUtente { get; set; }

        public void OnGet()
        {
            // Recupera i dati dell'utente dal contesto di autenticazione o da dove sono memorizzati
            // Ad esempio, potresti avere questi dati in un cookie o in una sessione
            CodiceFiscale = "ABC123XYZ4567890";
            NomeUtente = "Mario";
            CognomeUtente = "Rossi";
        }

        public IActionResult OnPostIscriviti(string partitaIva)
        {
            // Qui puoi gestire la logica di iscrizione dell'utente all'azienda con la Partita IVA fornita
            // Potresti chiamare un'API o eseguire altre operazioni di backend

            // Dopo l'iscrizione, reindirizza l'utente a una pagina di conferma o a una dashboard
            return RedirectToPage("/ConfermaIscrizione");
        }
    }
}
