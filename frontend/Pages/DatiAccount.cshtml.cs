using frontend.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace frontend.Pages
{
    public class DatiAccountModel : PageModel
    {
        public AziendaAgricolaModel? aziendaAgricola { get; set; }
        public AziendaIdricaModel? aziendaIdrica { get; set; }


        public async Task<IActionResult> OnGet()
        {
            /*
            // L'utente non è autenticato, reindirizzamento sulla pagina di login
            if (!IsUserAuth()) return RedirectToPage("/auth/SignIn");

            // Ottieni il CF dell'utente loggato
            string codFiscale = User.FindFirst("sub")?.Value;

            // Chiamata alle API per ottenere i dati
            if (codFiscale != null)
            {
                ApiReq.utente = await ApiReq.GetUserDataFromApi(codFiscale);
                if (ApiReq.utente.Role == "GSI")
                    aziendaIdrica = await ApiReq.GetAziendaIdricaDataFromApi(ApiReq.utente.PartitaIva);
                else
                    aziendaAgricola = await ApiReq.GetAziendaAgricolaDataFromApi(ApiReq.utente.PartitaIva);
            }
            else
            {
                return BadRequest();
            }
            */


            // Simulazione dati
            if (ApiReq.utente.Role == "GSI")
                aziendaIdrica = GetSimulatedAziendaIdricaData();
            else
                aziendaAgricola = GetSimulatedAziendaAgricolaData();


            // Continua con la generazione della pagina
            return Page();
        }



        

        // Funzione per effettuare il logout
        public async Task<IActionResult> OnPostLogout()
        {
            // Effettua il logout dell'utente
            await HttpContext.SignOutAsync();

            // Reindirizza all'azione di login o ad un'altra pagina di destinazione
            return RedirectToPage("/auth/SignIn");
        }

        // Controllo utente autenticato
        private bool IsUserAuth()
        {
            if (ApiReq.utente == null || User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return false;
            }
            return true;
        }

     
        // Simula i dati di un'azienda agricola
        private AziendaAgricolaModel GetSimulatedAziendaAgricolaData()
        {
            return new AziendaAgricolaModel
            {
                PartitaIva = "1234567890",
                Nome = "Azienda Agricola Rossi",
                Indirizzo = "Via delle Campagne, 123",
                Telefono = "0123456789",
                Email = "info@aziendaagricolarossi.com",
                Categoria = "AA", // AA / AI
                LimiteAcquistoAziendale = 5000.0f
            };
        }


        // Simula i dati di un'azienda idrica
        private AziendaIdricaModel GetSimulatedAziendaIdricaData()
        {   
            return new AziendaIdricaModel
            {
                PartitaIva = "9876543210",
                Nome = "Azienda Idrica Blu",
                Indirizzo = "Via dell'Acqua, 456",
                Telefono = "9876543210",
                Email = "info@aziendaidricablu.com",
                Categoria = "AI",
                LimiteErogazioneGlobale = 100000.0f
            };
        }




    }
}
