using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace frontend.Pages.GestoreIdrico
{
    public class RichiesteAdesioneModel : PageModel
    {
        public List<Utente> RichiesteUtenti { get; set; }
        public List<AziendaAgricolaModel> RichiesteAziendeAgricole { get; set; }

        public async Task<IActionResult> OnGet()
        {
            /*
            // L'utente non è autenticato, reindirizzamento sulla pagina di login
            if (!IsUserAuth()) return RedirectToPage("/auth/SignIn");

            // Imposta il token
            ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Token"]);

            
            RichiesteUtenti = await ApiReq.GetRichiesteUtentiFromApi(HttpContext);
            RichiesteAziendeAgricole = await ApiReq.GetRichiesteAziendeAgricoleFromApi(HttpContext);
            */


            // Simula dati di richieste
            RichiesteUtenti = GetSimulatedRichiesteUtenti();
            RichiesteAziendeAgricole = GetSimulatedRichiesteAziendeAgricole();

            return Page();
        }


        // Chiamata API per confermare un nuovo utente
        public async Task<IActionResult> OnPostConfermaUtente(string codiceFiscale)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaIdrica/offertaLimiti";

            /*
            // L'utente non è autenticato, reindirizzamento sulla pagina di login
            if (!IsUserAuth()) return RedirectToPage("/auth/SignIn");

            // Imposta il token
            ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Token"]);

            // Creare il corpo della richiesta
            var requestBody = new { CodiceFiscale = codiceFiscale };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata PUT
            HttpResponseMessage response = await ApiReq.httpClient.PutAsync(urlTask, content);

            if (response.IsSuccessStatusCode)
            {
                // Imposta un messaggio di successo
                TempData["Messaggio"] = "Conferma utente con codice fiscale: "+ codiceFiscale +" effettuata con successo!";
            }
            else
            {
                // Imposta un messaggio di errore
                TempData["MessaggioErrore"] = "Errore durante la conferma. Riprova più tardi.";
            }
            */

            TempData["MessaggioErrore"] = "Errore durante la conferma. Riprova più tardi.";

            return RedirectToPage();
        }


        // Chiamata API per confermare un nuovo utente
        public async Task<IActionResult> OnPostConfermaAzienda(string partitaIva)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaIdrica/offertaLimiti";

            /*
            // L'utente non è autenticato, reindirizzamento sulla pagina di login
            if (!IsUserAuth()) return RedirectToPage("/auth/SignIn");

            // Imposta il token
            ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Token"]);

            // Creare il corpo della richiesta
            var requestBody = new { PartitaIva = partitaIva };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata PUT
            HttpResponseMessage response = await ApiReq.httpClient.PutAsync(urlTask, content);

            if (response.IsSuccessStatusCode)
            {
                // Imposta un messaggio di successo
                TempData["Messaggio"] = "Conferma azienda con partita iva: " + partitaIva + " effettuata con successo!";
            }
            else
            {
                // Imposta un messaggio di errore
                TempData["MessaggioErrore"] = "Errore durante la conferma. Riprova più tardi.";
            }
            */

            TempData["MessaggioErrore"] = "Errore durante la conferma. Riprova più tardi.";

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





        // Simula dati di richieste utenti
        private List<Utente> GetSimulatedRichiesteUtenti()
        {
            return new List<Utente>
            {
                new Utente { CodiceFiscale = "ABC123", Nome = "Giovanni", Cognome = "Bianchi", PartitaIva = "123456789"},
                new Utente { CodiceFiscale = "XYZ456", Nome = "Maria", Cognome = "Rossi", PartitaIva = "888856789" },
                new Utente { CodiceFiscale = "DEF789", Nome = "Luca", Cognome = "Verdi", PartitaIva = "999956789" },
            };
        }

        // Simula dati di richieste aziende agricole
        private List<AziendaAgricolaModel> GetSimulatedRichiesteAziendeAgricole()
        {
            return new List<AziendaAgricolaModel>
            {
                new AziendaAgricolaModel { PartitaIva = "123456789", Nome = "Azienda Agricola Verde", Indirizzo = "Via Agricola 1", Telefono = "0123456789", Email = "azienda1@example.com", Categoria = "AA", LimiteAcquistoAziendale = 5000.0f},
                new AziendaAgricolaModel { PartitaIva = "987654321", Nome = "Azienda Agricola Sole", Indirizzo = "Via Agricola 2", Telefono = "9876543210", Email = "azienda2@example.com", Categoria = "AA", LimiteAcquistoAziendale = 8000.0f},
                new AziendaAgricolaModel { PartitaIva = "456789012", Nome = "Azienda Agricola Blu", Indirizzo = "Via Agricola 3", Telefono = "5678901234", Email = "azienda3@example.com", Categoria = "AA", LimiteAcquistoAziendale = 6000.0f},
            };
        }



    }
}
