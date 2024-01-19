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
        public List<UtenteAp>? RichiesteUtenti { get; set; }

        public List<UtentePeriodo> RichiestePeriodo { get; set; }
        public List<AziendaAgricolaModel> RichiesteAziendeAgricole { get; set; }

        public async Task<IActionResult> OnGet()
        {
            
            /*
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                RichiesteUtenti = await ApiReq.GetRichiesteUtentiFromApi(HttpContext);
                RichiestePeriodo = await ApiReq.GetRichiestePeriodoFromApi(HttpContext);
                RichiesteAziendeAgricole = await ApiReq.GetRichiesteAziendeAgricoleFromApi(HttpContext);
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
            RichiesteAziendeAgricole = GetSimulatedRichiesteAziendeAgricole();

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
                    TempData["Messaggio"] = "Richiesta id: " + id + " impostata con stato: " + action;
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

            TempData["Messaggio"] = "Richiesta id: " + id + " impostata con stato: " + action;
            TempData["MessaggioErrore"] = "Errore durante la l'accettazione/rifiuto della richiesta. Riprova più tardi.";

            return RedirectToPage();
        }

        // Chiamata API per confermare un nuovo utente
        public async Task<IActionResult> OnPostConfermaUtentePeriodo(string codiceFiscale)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaIdrica/confermaUtentePeriodo";
            
            /*
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

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
                    TempData["Messaggio"] = "Conferma periodo di accesso utente con codice fiscale: " + codiceFiscale + " effettuata con successo!";
                }
                else
                {
                    // Imposta un messaggio di errore
                    TempData["MessaggioErrore"] = "Errore durante la conferma. Riprova più tardi.";
                }
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
            */

            TempData["Messaggio"] = "Conferma periodo di accesso utente con codice fiscale: " + codiceFiscale + " effettuata con successo!";
            TempData["MessaggioErrore"] = "Errore durante la conferma. Riprova più tardi.";

            return RedirectToPage();
        }


        // Chiamata API per confermare un nuovo utente
        public async Task<IActionResult> OnPostConfermaAzienda(string partitaIva)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaIdrica/confermaAzienda";
            
            /*
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

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
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
            */

            TempData["Messaggio"] = "Conferma azienda con partita iva: " + partitaIva + " effettuata con successo!";
            TempData["MessaggioErrore"] = "Errore durante la conferma. Riprova più tardi.";

            return RedirectToPage();
        }





        // Simula dati di richieste utenti
        private List<UtenteAp> GetSimulatedRichiesteUtenti()
        {
            return new List<UtenteAp>
            {
                new UtenteAp {Id="1", CodiceFiscale = "ABC123", Nome = "Giovanni", Cognome = "Bianchi", PartitaIva = "123456789", TipoAzienda="WSP"},
                new UtenteAp {Id="2", CodiceFiscale = "XYZ456", Nome = "Maria", Cognome = "Rossi", PartitaIva = "888856789", TipoAzienda="FAR" },
                new UtenteAp {Id="3", CodiceFiscale = "DEF789", Nome = "Luca", Cognome = "Verdi", PartitaIva = "999956789", TipoAzienda="FAR" },
            };
        }


        // Simula dati di richieste utenti periodo accesso alle api
        private List<UtentePeriodo> GetSimulatedRichiestePeriodo()
        {
            return new List<UtentePeriodo>
            {
                new UtentePeriodo { CodiceFiscale = "ABC123", Nome = "Giovanni", Cognome = "Bianchi", PartitaIva = "123456789", DataInizio = "2024-02-12", DataFine = "2024-03-12"},
                new UtentePeriodo { CodiceFiscale = "XYZ456", Nome = "Maria", Cognome = "Rossi", PartitaIva = "888856789", DataInizio = "2024-03-22", DataFine = "2024-03-23" },
                new UtentePeriodo { CodiceFiscale = "DEF789", Nome = "Luca", Cognome = "Verdi", PartitaIva = "999956789", DataInizio = "2024-01-29", DataFine = "2025-01-29" },
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
