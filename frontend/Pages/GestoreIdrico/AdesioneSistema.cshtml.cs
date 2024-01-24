using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace frontend.Pages.GestoreIdrico
{
    public class AdesioneSistemaModel : PageModel
    {
        public List<UtenteAp>? RichiesteUtenti { get; set; }

        public List<UtentePeriodo> RichiestePeriodo { get; set; }

        public async Task<IActionResult> OnGet()
        {

            /*
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                // Controllo utente autorizzato
                if (ApiReq.utente.Role!="WSP") { throw new Exception("Unauthorized"); }

                RichiesteUtenti = await ApiReq.GetRichiesteUtentiFromApi(HttpContext);
                RichiestePeriodo = await ApiReq.GetRichiestePeriodoFromApi(HttpContext);
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
            */

            // Simula dati di richieste
            RichiesteUtenti = GetSimulatedRichiesteUtenti();
            RichiestePeriodo = GetSimulatedRichiestePeriodo();

            return Page();
        }


        // Chiamata API per confermare o rifiutare un nuovo utente
        public async Task<IActionResult> OnPostConfermaUtente(string id, string action)
        {
            string urlTask = ApiReq.urlGenerico + $"/service/application/{id}/{action}";

            /*
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                // Controllo utente autorizzato
                if (ApiReq.utente.Role!="WSP") { throw new Exception("Unauthorized"); }

                // Imposta il token
                ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Token"]);

                // Creare il corpo della richiesta
                var requestBody = new { id = id, action = action };
                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Esegue la chiamata
                HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

                if (response.IsSuccessStatusCode)
                {
                    // Imposta un messaggio di successo
                    TempData["Messaggio"] = "Richiesta adesione con id: " + id + " impostata con stato: " + action;
                }
                else
                {
                    // Imposta un messaggio di errore
                    TempData["MessaggioErrore"] = "Errore durante la l'accettazione/rifiuto della richiesta. Riprova più tardi.";
                }
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
            */

            TempData["Messaggio"] = "Richiesta adesione con id: " + id + " impostata con stato: " + action;
            TempData["MessaggioErrore"] = "Errore durante la l'accettazione/rifiuto della richiesta. Riprova più tardi.";

            return RedirectToPage();
        }

        // Chiamata API per confermare un nuovo utente
        public async Task<IActionResult> OnPostConfermaUtentePeriodo(string id, string action)
        {
            string urlTask = ApiReq.urlGenerico + $"/apiaccess/{id}/{action}";

            /*
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                // Controllo utente autorizzato
                if (ApiReq.utente.Role!="WSP") { throw new Exception("Unauthorized"); }

                // Imposta il token
                ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Token"]);

                // Creare il corpo della richiesta
                var requestBody = new { id = id, action = action };
                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Esegue la chiamata PUT
                HttpResponseMessage response = await ApiReq.httpClient.PutAsync(urlTask, content);

                if (response.IsSuccessStatusCode)
                {
                    // Imposta un messaggio di successo
                    TempData["Messaggio"] = "Richiesta periodo con id: " + id + " impostata con stato: " + action;
                }
                else
                {
                    // Imposta un messaggio di errore
                    TempData["MessaggioErrore"] = "Errore durante la conferma del periodo. Riprova più tardi.";
                }
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
            */

            TempData["Messaggio"] = "Richiesta periodo con id: " + id + " impostata con stato: " + action;
            TempData["MessaggioErrore"] = "Errore durante la conferma del periodo. Riprova più tardi.";

            return RedirectToPage();
        }



        // Simula dati di richieste utenti
        private List<UtenteAp> GetSimulatedRichiesteUtenti()
        {
            return new List<UtenteAp>
            {
                new UtenteAp {Id="1", Email="giovanni@mail.mail", CodiceFiscale = "ABC123", Nome = "Giovanni", Cognome = "Bianchi", PartitaIva = "123456789", TipoAzienda="WSP"},
                new UtenteAp {Id="2", Email="maria@mail.mail", CodiceFiscale = "XYZ456", Nome = "Maria", Cognome = "Rossi", PartitaIva = "888856789", TipoAzienda="FAR" },
                new UtenteAp {Id="3", Email="luca@mail.mail", CodiceFiscale = "DEF789", Nome = "Luca", Cognome = "Verdi", PartitaIva = "999956789", TipoAzienda="FAR" },
            };
        }


        // Simula dati di richieste utenti periodo accesso alle api
        private List<UtentePeriodo> GetSimulatedRichiestePeriodo()
        {
            return new List<UtentePeriodo>
            {
                new UtentePeriodo { Id="4", Email="giovanni@mail.mail", CodiceFiscale = "CBC123", Nome = "Giovanni", Cognome = "Bianchi", PartitaIva = "123456789", DataInizio = "2024-02-12", DataFine = "2024-03-12"},
                new UtentePeriodo { Id="5", Email="maria@mail.mail", CodiceFiscale = "CYZ456", Nome = "Maria", Cognome = "Rossi", PartitaIva = "888856789", DataInizio = "2024-03-22", DataFine = "2024-03-23" },
                new UtentePeriodo { Id="6", Email="luca@mail.mail",CodiceFiscale = "CEF789", Nome = "Luca", Cognome = "Verdi", PartitaIva = "999956789", DataInizio = "2024-01-29", DataFine = "2025-01-29" },
            };
        }


    }
}
