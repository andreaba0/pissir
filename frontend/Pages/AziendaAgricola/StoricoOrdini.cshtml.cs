using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace frontend.Pages.AziendaAgricola
{
    public class StoricoOrdiniModel : PageModel
    {
        public List<OrdineAcquisto>? StoricoOrdini { get; set; }

        public async Task<IActionResult> OnGet()
        {
            
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                // Controllo utente autorizzato
                if (ApiReq.utente.Role!="FA") { return RedirectToPage("/DatiAccount"); }

                //StoricoOrdini = GetStoricoOrdini();
                //return Page();

                // Richiesta API
                string data = await ApiReq.GetDataFromApi(HttpContext, "/water/order", true, true);
                StoricoOrdini = JsonConvert.DeserializeObject<List<OrdineAcquisto>>(data);

                return Page();
            }
            catch (HttpRequestException ex)
            {
                string statusCode = ex.Message.ToString().ToLower();

                if (statusCode == "unauthorized")
                {
                    // Errore 401
                    TempData["MessaggioErrore"] = "Non sei autorizzato. Effettua prima la richiesta di accesso ai servizi.";
                    return RedirectToPage("/Error");
                }
                else
                {
                    TempData["MessaggioErrore"] = $"Errore: {ex.Message}. Riprovare pi� tardi.";
                    return RedirectToPage("/Error");
                }
            }
        }



        // Metodo di esempio per ottenere dati di storico ordini
        private List<OrdineAcquisto> GetStoricoOrdini()
        {
            return new List<OrdineAcquisto>
            {
                new OrdineAcquisto
                {
                    IdOfferta = "1",
                    PartitaIva = "1234567890",
                    IdCampo = "Campo1",
                    Quantita = 30.2f
                },
                new OrdineAcquisto
                {
                    IdOfferta = "2",
                    PartitaIva = "1234567890",
                    IdCampo = "Campo2",
                    Quantita = 56f
                },
                new OrdineAcquisto
                {
                    IdOfferta = "3",
                    PartitaIva = "1234567890",
                    IdCampo = "Campo1",
                    Quantita = 6f
                }

            };
        }
    }
}
