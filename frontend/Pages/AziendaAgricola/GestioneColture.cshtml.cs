using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace frontend.Pages.AziendaAgricola
{
    public class GestioneColtureModel : PageModel
    {
        public List<Coltura>? Colture { get; set; }


        // Chiamata API per lista colture
        public async Task<IActionResult> OnGet()
        {
            /*
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                // Controllo utente autorizzato
                if (ApiReq.utente.Role!="FAR") { throw new Exception("Unauthorized"); }

                // Chiamata alle API per ottenere i dati
                Colture = await ApiReq.GetColtureAziendaFromApi(HttpContext);
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
            */

            // Simulazione dati
            Colture = GetListaColture();

            return Page();
        }



        // Chiamata API per aggiunta coltura
        public async Task<IActionResult> OnPostAggiungiColtura(string metriQuadrati, string tipoColtura, string tipoIrrigazione)
        {
            string urlTask = ApiReq.urlGenerico + "/field";

            /*
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                // Controllo utente autorizzato
                if (ApiReq.utente.Role!="FAR") { throw new Exception("Unauthorized"); }

                // Imposta il token
                ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Token"]);

                // Creare il corpo della richiesta
                var requestBody = new
                {
                    square_meters = metriQuadrati,
                    crop_type = tipoColtura,
                    irrigation_type = tipoIrrigazione
                };
                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Esegue la chiamata PUT per l'aggiunta della coltura
                HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

                if (response.IsSuccessStatusCode)
                {
                    // Imposta un messaggio di successo
                    TempData["Messaggio"] = "Aggiunta della coltura effettuata con successo!";
                }
                else
                {
                    // Imposta un messaggio di errore
                    TempData["MessaggioErrore"] = "Errore durante l'aggiunta della coltura. Riprova più tardi.";
                }
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
            */

            TempData["MessaggioErrore"] = "Errore durante l'aggiunta della coltura. Riprova più tardi.";

            return RedirectToPage();
        }


        // Chiamata API per modifica coltura
        public async Task<IActionResult> OnPostModificaColtura(string metriQuadrati, string tipoColtura, string tipoIrrigazione, string colturaId)
        {
            string urlTask = ApiReq.urlGenerico + $"/field/{colturaId}";

            /*
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                // Controllo utente autorizzato
                if (ApiReq.utente.Role!="FAR") { throw new Exception("Unauthorized"); }    

                // Imposta il token
                ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Token"]);

                // Creare il corpo della richiesta
                var requestBody = new
                {
                    square_meters = metriQuadrati,
                    crop_type = tipoColtura,
                    irrigation_type = tipoIrrigazione
                };
                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Esegue la chiamata POST
                HttpResponseMessage response = await ApiReq.httpClient.PatchAsync(urlTask, content);

                if (response.IsSuccessStatusCode)
                {
                    // Imposta un messaggio di successo
                    TempData["Messaggio"] = "Modifica della coltura con ID = " + colturaId + " effettuata con successo!";
                }
                else
                {
                    // Imposta un messaggio di errore
                    TempData["MessaggioErrore"] = "Errore durante la modifica. Riprova più tardi.";
                }
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
            */

            TempData["MessaggioErrore"] = "Errore durante la modifica. Riprova più tardi.";

            await OnGet();
            return RedirectToPage();
        }


        // Chiamata API per eliminazione coltura
        public async Task<IActionResult> OnPostEliminaColtura(string colturaId)
        {
            string urlTask = ApiReq.urlGenerico + $"/field/{colturaId}";

            /*
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");
                
                // Controllo utente autorizzato
                if (ApiReq.utente.Role!="FAR") { throw new Exception("Unauthorized"); }
                
                // Imposta il token
                ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Token"]);

                // Esegue la chiamata DELETE per l'eliminazione della coltura
                HttpResponseMessage response = await ApiReq.httpClient.DeleteAsync(urlTask);

                if (response.IsSuccessStatusCode)
                {
                    // Imposta un messaggio di successo
                    TempData["Messaggio"] = "Eliminazione della coltura con ID = " + colturaId + " effettuata con successo!";
                }
                else
                {
                    // Imposta un messaggio di errore
                    TempData["MessaggioErrore"] = "Errore durante l'eliminazione della coltura. Riprova più tardi.";
                }
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
            */

            TempData["MessaggioErrore"] = "Errore durante l'eliminazione della coltura. Riprova più tardi.";

            return RedirectToPage();
        }






        // Simulazione dati
        private List<Coltura> GetListaColture()
        {
            // Simulazione di dati
            return new List<Coltura>
            {
                new Coltura { Id = "1", PartitaIva = "12345678901", MetriQuadrati = 100, TipoColtura = "Grano", TipoIrrigazione = "Goccia" },
                new Coltura { Id = "2", PartitaIva = "12345678901", MetriQuadrati = 200, TipoColtura = "Sorgo", TipoIrrigazione = "Goccia" },
                new Coltura { Id = "3", PartitaIva = "12345678901", MetriQuadrati = 150, TipoColtura = "Mais", TipoIrrigazione = "Spruzzo" },
                new Coltura { Id = "4", PartitaIva = "12345678901", MetriQuadrati = 10, TipoColtura = "Cotone", TipoIrrigazione = "Goccia" }
            };
        }
    }

}
