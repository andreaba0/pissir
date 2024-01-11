using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace frontend.Pages.GestoreIdrico
{
    public class StoricoVenditeModel : PageModel
    {
        public List<OrdineAcquisto>? Acquisti { get; set; }

        public async Task<IActionResult> OnGet()
        {
            /*
            // L'utente non è autenticato, reindirizzamento sulla pagina di login
            if (!IsUserAuth()) return RedirectToPage("/auth/SignIn");

            // Imposta il token
            ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Token"]);

            
            ApiReq.utente = await ApiReq.GetUserDataFromApi(HttpContext);
            Acquisti = await ApiReq.GetStoricoVenditeFromApi(ApiReq.utente.PartitaIva, HttpContext);
            */

            // Simulazione dati
            Acquisti = GetListaAcquisti();

            return Page();
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





        // Simulazione di dati
        private List<OrdineAcquisto> GetListaAcquisti()
        {
            
            return new List<OrdineAcquisto>
            {
                new OrdineAcquisto { IdOfferta = "Offer1", PartitaIva="2793902840", IdCampo = "Campo1", Quantita = 10.5f },
                new OrdineAcquisto { IdOfferta = "Offer2", PartitaIva="2793902840", IdCampo = "Campo2", Quantita = 8.2f },
                new OrdineAcquisto { IdOfferta = "Offer3", PartitaIva="7676669849", IdCampo = "Campo3", Quantita = 4.5f },
                new OrdineAcquisto { IdOfferta = "Offer4", PartitaIva="4567445112", IdCampo = "Campo4", Quantita = 10.9f },

            };
        }
    }
}