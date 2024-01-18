using frontend.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;

namespace frontend.Pages
{
    public class DatiAccountModel : PageModel
    {
        public AziendaAgricolaModel? aziendaAgricola { get; set; }
        public AziendaIdricaModel? aziendaIdrica { get; set; }

        public async Task<IActionResult> OnGet()
        {
            /*
            // Controllo utente autenticato
            if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");


            try
            {
                // Chiamata alle API per ottenere i dati
                ApiReq.utente = await ApiReq.GetUserDataFromApi(HttpContext);
                if (ApiReq.utente.Role == "WSP")
                    aziendaIdrica = await ApiReq.GetAziendaIdricaDataFromApi(HttpContext);
                else if (ApiReq.utente.Role == "FAR")
                    aziendaAgricola = await ApiReq.GetAziendaAgricolaDataFromApi(HttpContext);
                else
                    throw new Exception("Errore nel prelevare i dati dell'account.");
            }
            catch (Exception ex) 
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
            */
            
            


            // Simulazione dati          
            if (ApiReq.utente.Role == "WSP")
                aziendaIdrica = GetSimulatedAziendaIdricaData();
            else
                aziendaAgricola = GetSimulatedAziendaAgricolaData();
            
            // Continua con la generazione della pagina
            return Page();
        }


        // Funzione per effettuare il logout
        public async Task<IActionResult> OnPostLogout()
        {
            // Cancella il cookie del token
            Response.Cookies.Delete("Token");

            // Reindirizza alla pagina di accesso
            return RedirectToPage("/auth/SignIn");
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
                Categoria = "FA",
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
                Categoria = "WA",
                LimiteErogazioneGlobale = 100000.0f
            };
        }




    }
}
