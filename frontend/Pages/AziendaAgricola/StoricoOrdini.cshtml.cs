using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace frontend.Pages.AziendaAgricola
{
    public class StoricoOrdiniModel : PageModel
    {
        public List<OrdineAcquisto>? StoricoOrdini { get; set; }

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
                StoricoOrdini = await ApiReq.GetStoricoOrdiniFromApi(ApiReq.utente.PartitaIva);
            }
            else
            {
                return BadRequest();
            }
            */

            // Simulazione dati
            StoricoOrdini = GetStoricoOrdini();

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
