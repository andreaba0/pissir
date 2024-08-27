using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace frontend.Pages.GestoreIdrico
{
    public class StoricoVenditeModel : PageModel
    {
        public List<OrdineAcquisto>? Acquisti { get; set; }

        public async Task<IActionResult> OnGet()
        {
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                // Controllo utente autorizzato
                if (ApiReq.utente.Role!="WA") { throw new Exception("Unauthorized"); }

                // Simulazione dati
                Acquisti = GetListaAcquisti();

                return Page();

                // Richiesta API
                string data = await ApiReq.GetDataFromApi(HttpContext, "/water/order");
                Acquisti = JsonConvert.DeserializeObject<List<OrdineAcquisto>>(data);
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
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