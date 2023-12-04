using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Text;

namespace frontend.Pages.GestoreIdrico
{
    public class ConsumiAziendeModel : PageModel
    {        
        public List<ConsumoAziendaleCampo>? consumiAziende { get; set; }

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
                consumiAziende = await ApiReq.GetConsumoAziendeFromApi(ApiReq.utente.PartitaIva);
            }
            else
            {
                return BadRequest();
            }
            */

            // Simulazione Dati
            consumiAziende = GetStoricoConsumi();

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







        // Metodo di esempio per ottenere dati di storico consumi
        private List<ConsumoAziendaleCampo> GetStoricoConsumi()
        {
            // Implementa la logica per recuperare i dati di storico consumi
            // dal tuo servizio o repository dati
            // Restituisci una lista di oggetti ConsumoAziendaleCampo
            return new List<ConsumoAziendaleCampo>
            {
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "123456789",
                    Campo = "Campo1",
                    Data = "2023-01-01",
                    LitriConsumati = 100.5f,
                    QuantitaPrenotata = 50.2f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "344535334",
                    Campo = "Campo2",
                    Data = "2023-02-15",
                    LitriConsumati = 75.2f,
                    QuantitaPrenotata = 30.0f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "575757576",
                    Campo = "Campo3",
                    Data = "2023-03-20",
                    LitriConsumati = 120.8f,
                    QuantitaPrenotata = 60.1f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "246378676",
                    Campo = "Campo4",
                    Data = "2023-04-10",
                    LitriConsumati = 90.0f,
                    QuantitaPrenotata = 45.5f
                }
            };
        }
    }
}
