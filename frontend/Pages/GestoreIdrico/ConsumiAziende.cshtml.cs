using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace frontend.Pages.GestoreIdrico
{
    public class ConsumiAziendeModel : PageModel
    {        
        public List<ConsumoAziendaleCampo>? consumiAziende { get; set; }

        public async Task<IActionResult> OnGet()
        {
            
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                // Controllo utente autorizzato
                if (ApiReq.utente.Role!="WA") { throw new Exception("Unauthorized"); }

                // Simulazione Dati
                consumiAziende = GetStoricoConsumi();

                return Page();

                // Richiesta API
                string data = await ApiReq.GetDataFromApi(HttpContext, "/water/consumption", true, true);
                consumiAziende = JsonConvert.DeserializeObject<List<ConsumoAziendaleCampo>>(data);
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
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
                    LitriConsumati = 145.5f,
                    QuantitaPrenotata = 150.2f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "123456789",
                    Campo = "Campo1",
                    Data = "2023-01-02",
                    LitriConsumati = 45.5f,
                    QuantitaPrenotata = 60.2f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "123456789",
                    Campo = "Campo2",
                    Data = "2023-02-15",
                    LitriConsumati = 95.2f,
                    QuantitaPrenotata = 130.0f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "575757576",
                    Campo = "Campo3",
                    Data = "2023-03-20",
                    LitriConsumati = 120.8f,
                    QuantitaPrenotata = 160.1f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "575757576",
                    Campo = "Campo2",
                    Data = "2023-03-20",
                    LitriConsumati = 120.8f,
                    QuantitaPrenotata = 160.1f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "575757576",
                    Campo = "Campo3",
                    Data = "2023-03-20",
                    LitriConsumati = 130.8f,
                    QuantitaPrenotata = 160.1f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "575757576",
                    Campo = "Campo3",
                    Data = "2023-03-20",
                    LitriConsumati = 20.8f,
                    QuantitaPrenotata = 60.1f
                },
                new ConsumoAziendaleCampo
                {
                    PartitaIva = "246378676",
                    Campo = "Campo4",
                    Data = "2023-04-10",
                    LitriConsumati = 40.0f,
                    QuantitaPrenotata = 45.5f
                }
            };
        }
    }
}
