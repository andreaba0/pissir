using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace frontend.Pages.AziendaAgricola
{
    public class StoricoConsumiModel : PageModel
    {
        public List<ConsumoAziendaleCampo>? StoricoConsumi { get; set; }

        public async Task<IActionResult> OnGet()
        {
            /*
            // L'utente non è autenticato, reindirizzamento sulla pagina di login
            if (!IsUserAuth()) return RedirectToPage("/auth/SignIn");

            // Imposta il token
            ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["AccessToken"]);

            // Ottieni il CF dell'utente loggato
            string codFiscale = User.FindFirst("sub")?.Value;

            // Chiamata alle API per ottenere i dati
            if (codFiscale != null)
            {
                ApiReq.utente = await ApiReq.GetUserDataFromApi(codFiscale, HttpContext);
                StoricoConsumi = await ApiReq.GetStoricoConsumiFromApi(ApiReq.utente.PartitaIva, HttpContext);
            }
            else
            {
                return BadRequest();
            }
            */

            // Simulazione dati
            StoricoConsumi = GetStoricoConsumi();

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
            return new List<ConsumoAziendaleCampo>
            {
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "123456789",
                    Campo = "Campo1",
                    Data = "2023-01-01",
                    LitriConsumati = 100.5f,
                    QuantitaPrenotata = 150.2f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "123456789",
                    Campo = "Campo2",
                    Data = "2023-02-15",
                    LitriConsumati = 75.2f,
                    QuantitaPrenotata = 130.0f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "123456789",
                    Campo = "Campo3",
                    Data = "2023-03-20",
                    LitriConsumati = 120.8f,
                    QuantitaPrenotata = 130.1f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "123456789",
                    Campo = "Campo4",
                    Data = "2023-04-10",
                    LitriConsumati = 90.0f,
                    QuantitaPrenotata = 90.5f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "123456789",
                    Campo = "Campo5",
                    Data = "2023-05-05",
                    LitriConsumati = 10.0f,
                    QuantitaPrenotata = 45.5f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "123456789",
                    Campo = "Campo6",
                    Data = "2023-06-15",
                    LitriConsumati = 30.2f,
                    QuantitaPrenotata = 45.5f
                },
                // Aggiungi altri dati per i campi da 1 a 6
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "123456789",
                    Campo = "Campo1",
                    Data = "2023-07-01",
                    LitriConsumati = 80.0f,
                    QuantitaPrenotata = 110.5f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "123456789",
                    Campo = "Campo2",
                    Data = "2023-08-10",
                    LitriConsumati = 55.5f,
                    QuantitaPrenotata = 95.2f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "123456789",
                    Campo = "Campo3",
                    Data = "2023-09-15",
                    LitriConsumati = 105.3f,
                    QuantitaPrenotata = 120.0f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "123456789",
                    Campo = "Campo4",
                    Data = "2023-10-20",
                    LitriConsumati = 70.5f,
                    QuantitaPrenotata = 85.0f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "123456789",
                    Campo = "Campo5",
                    Data = "2023-11-25",
                    LitriConsumati = 15.8f,
                    QuantitaPrenotata = 50.5f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "123456789",
                    Campo = "Campo6",
                    Data = "2023-12-31",
                    LitriConsumati = 40.2f,
                    QuantitaPrenotata = 60.5f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "123456789",
                    Campo = "Campo1",
                    Data = "2023-11-25",
                    LitriConsumati = 15.8f,
                    QuantitaPrenotata = 50.5f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "123456789",
                    Campo = "Campo3",
                    Data = "2023-12-31",
                    LitriConsumati = 40.2f,
                    QuantitaPrenotata = 60.5f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "123456789",
                    Campo = "Campo1",
                    Data = "2023-11-26",
                    LitriConsumati = 15.8f,
                    QuantitaPrenotata = 50.5f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "123456789",
                    Campo = "Campo3",
                    Data = "2023-12-30",
                    LitriConsumati = 40.2f,
                    QuantitaPrenotata = 60.5f
                }
                // Aggiungi altri dati per i campi da 1 a 6 se necessario
            };
        }






    }
}