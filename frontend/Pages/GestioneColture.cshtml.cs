using System.Collections.Generic;
using System.Text;
using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace frontend.Pages
{
    public class GestioneColtureModel : PageModel
    {
        public List<Coltura>? Colture { get; set; }


        // Chiamata API per lista colture
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
                Colture = await ApiReq.GetColtureAziendaFromApi(ApiReq.utente.PartitaIva);
            }
            else
            {
                return BadRequest();
            }
            */

            // Simulazione dati
            Colture = GetListaColture();

            return Page();
        }



        // Chiamata API per aggiunta coltura
        public async Task<IActionResult> OnPostAggiungiColtura(string partitaIva, string metriQuadrati, string tipoColtura, string tipoIrrigazione)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaAgricola/coltura";

            /*
            // L'utente non è autenticato, reindirizzamento sulla pagina di login
            if (!IsUserAuth()) return RedirectToPage("/auth/SignIn");

            // Creare il corpo della richiesta
            var requestBody = new
            {
                PartitaIva = partitaIva,
                MetriQuadrati = metriQuadrati,
                TipoColtura = tipoColtura,
                TipoIrrigazione = tipoIrrigazione
            };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata PUT per l'aggiunta della coltura
            HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

            if (response.IsSuccessStatusCode)
            {
                // Imposta un messaggio di successo
                TempData["Messaggio"] = "Aggiunta della coltura per l'azienda P.Iva "+ partitaIva +" effettuata con successo!";
            }
            else
            {
                // Imposta un messaggio di errore
                TempData["MessaggioErrore"] = "Errore durante l'aggiunta della coltura. Riprova più tardi.";
            }
            */

            TempData["MessaggioErrore"] = "Errore durante l'aggiunta della coltura. Riprova più tardi.";

            return RedirectToPage();
        }


        // Chiamata API per modifica coltura
        public async Task<IActionResult> OnPostModificaColtura(string metriQuadrati, string tipoColtura, string tipoIrrigazione, string colturaId)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaAgricola/coltura";

            /*
            // L'utente non è autenticato, reindirizzamento sulla pagina di login
            if (!IsUserAuth()) return RedirectToPage("/auth/SignIn");
            
            // Puoi impostare eventuali intestazioni necessarie qui
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "IlTuoToken");

            // Creare il corpo della richiesta
            var requestBody = new 
            { 
                PartitaIva = ApiReq.utente.PartitaIva,
                ColturaId = colturaId,
                MetriQuadrati = metriQuadrati,
                TipoColtura = tipoColtura,
                TipoIrrigazione = tipoIrrigazione
            };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata POST
            HttpResponseMessage response = await ApiReq.httpClient.PutAsync(urlTask, content);
            
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
            */
            TempData["MessaggioErrore"] = "Errore durante la modifica. Riprova più tardi.";

            await OnGet();
            return RedirectToPage();
        }


        // Chiamata API per eliminazione coltura
        public async Task<IActionResult> OnPostEliminaColtura(string colturaId)
        {
            string urlTask = $"{ApiReq.urlGenerico}aziendaAgricola/coltura/?IdColtura={Uri.EscapeDataString(colturaId)}";

            /*
            // L'utente non è autenticato, reindirizzamento sulla pagina di login
            if (!IsUserAuth()) return RedirectToPage("/auth/SignIn");
            
            // Esegue la chiamata DELETE per l'eliminazione della coltura
            HttpResponseMessage response = await ApiReq.httpClient.DeleteAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Imposta un messaggio di successo
                TempData["Messaggio"] = "Eliminazione della coltura con ID = "+ colturaId +" effettuata con successo!";
            }
            else
            {
                // Imposta un messaggio di errore
                TempData["MessaggioErrore"] = "Errore durante l'eliminazione della coltura. Riprova più tardi.";
            }
            */
            TempData["MessaggioErrore"] = "Errore durante l'eliminazione della coltura. Riprova più tardi.";

            return RedirectToPage();
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
