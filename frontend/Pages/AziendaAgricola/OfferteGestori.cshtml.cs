using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace frontend.Pages.AziendaAgricola
{
    public class OfferteGestoriModel : PageModel
    {
        public List<Offerta>? Offerte { get; set; }
        public List<Coltura>? Colture { get; set; }


        public async Task<IActionResult> OnGet()
        {
            /*
            // Controllo utente autenticato
            if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

            // Imposta il token
            ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Token"]);

            
            Offerte = await ApiReq.GetOfferteIdricheFromApi(HttpContext);
            ApiReq.utente = await ApiReq.GetUserDataFromApi(HttpContext);
            Colture = await ApiReq.GetColtureAziendaFromApi(ApiReq.utente.PartitaIva, HttpContext);
            Colture = GetListaColture();
           
            */

            // Simulazione dati
            Offerte = GetListaOfferte();
            Colture = GetListaColture();

            return Page();
        }


        // Chiamata API per acquisto risorse idriche
        public async Task<IActionResult> OnPostOrdineAcquisto(string offertaId, string quantitaAcquisto)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaAgricola/offerteIdriche";

            /*
            // Controllo utente autenticato
            if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

            // Imposta il token
            ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Token"]);

            // Creare il corpo della richiesta
            var requestBody = new
            {
                OffertaId = offertaId,
                QuantitaAcquisto = quantitaAcquisto
            };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata POST
            HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

            if (response.IsSuccessStatusCode)
            {
                // Imposta un messaggio di successo
                TempData["Messaggio"] = "Risorse acquistate dall'offerta con ID = " + offertaId ;
            }
            else
            {
                // Imposta un messaggio di errore
                TempData["MessaggioErrore"] = "Errore durante l'acquisto. Riprova più tardi.";
            }
            */
            TempData["MessaggioErrore"] = "Errore durante l'acquisto. Riprova più tardi.";

            await OnGet();
            return RedirectToPage();
        }




        // Simulazione dati
        private List<Offerta> GetListaOfferte()
        {
            // Simulazione di dati
            return new List<Offerta>
            {
                new Offerta { Id = "1", PartitaIva = "12345678901", DataAnnuncio = "2023-11-03", PrezzoLitro = 1.5f, Quantita = 100 },
                new Offerta { Id = "2", PartitaIva = "67895678901", DataAnnuncio = "2023-11-04", PrezzoLitro = 1.8f, Quantita = 150 },
                new Offerta { Id = "3", PartitaIva = "98765432109", DataAnnuncio = "2023-11-05", PrezzoLitro = 2.0f, Quantita = 120 }
            };
        }

        // Simulazione dati
        private List<Coltura> GetListaColture()
        {
            // Simulazione di dati
            return new List<Coltura>
            {
                new Coltura { Id = "1", PartitaIva = "12345678901", MetriQuadrati = 100, TipoColtura = "Grano", TipoIrrigazione = "Goccia" },
                new Coltura { Id = "2", PartitaIva = "12345678901", MetriQuadrati = 200, TipoColtura = "Sorgo", TipoIrrigazione = "Goccia" },
                new Coltura { Id = "3", PartitaIva = "12345678901", MetriQuadrati = 150, TipoColtura = "Mais", TipoIrrigazione = "Spruzzo" }
            };
        }


    }
}
