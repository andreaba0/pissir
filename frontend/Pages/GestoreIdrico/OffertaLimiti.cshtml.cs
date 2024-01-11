using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using frontend.Models;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;

namespace frontend.Pages.GestoreIdrico
{
    public class OffertaLimitiModel : PageModel
    {
        public List<LimiteAcquistoAzienda>? LimitiAcquistoPerAzienda { get; set; }
        public float AcquaDisponibile { get; set; }
        public float LimiteGiornalieroVendita { get; set; }

        public async Task<IActionResult> OnGet()
        {
            /*
            // L'utente non è autenticato, reindirizzamento sulla pagina di login
            if (!IsUserAuth()) return RedirectToPage("/auth/SignIn");

            // Imposta il token
            ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Token"]);

            
            ApiReq.utente = await ApiReq.GetUserDataFromApi(HttpContext);
            AcquaDisponibile = await ApiReq.GetAcquaDisponibile(ApiReq.utente.PartitaIva, HttpContext);
            LimiteGiornalieroVendita = await ApiReq.GetLimiteGiornaliero(ApiReq.utente.PartitaIva, HttpContext);
            LimitiAcquistoPerAzienda = await ApiReq.GetLimitiPerAziendaFromApi(HttpContext);
            
            */

            // Simula i dati di esempio
            SimulaDatiDiEsempio();

            return Page();
        }

        
        // Chiamata API per modificare l'offerta idrica
        public async Task<IActionResult> OnPostModificaOfferta(string quantitaAcqua, string limiteGiornaliero)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaIdrica/offertaLimiti";

            /*
            // L'utente non è autenticato, reindirizzamento sulla pagina di login
            if (!IsUserAuth()) return RedirectToPage("/auth/SignIn");

            // Imposta il token
            ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Token"]);

            // Creare il corpo della richiesta
            var requestBody = new
            {
                PartitaIva = ApiReq.utente.PartitaIva,
                QuantitaAcqua = quantitaAcqua,
                LimiteGiornaliero = limiteGiornaliero
            };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata PUT
            HttpResponseMessage response = await ApiReq.httpClient.PutAsync(urlTask, content);

            if (response.IsSuccessStatusCode)
            {
                // Imposta un messaggio di successo
                TempData["Messaggio"] = "Modifica offerta e limite effettuata con successo!";
            }
            else
            {
                // Imposta un messaggio di errore
                TempData["MessaggioErrore"] = "Errore durante la modifica. Riprova più tardi.";
            }
            */
            TempData["MessaggioErrore"] = "Errore durante la modifica. Riprova più tardi.";

            return RedirectToPage();
        }

        // Chiamata API per modificare il limite di acquisto per le aziende agricole
        public async Task<IActionResult> OnPostModificaLimiteAziendale(string nuovoLimite, string partitaIvaAzienda)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaIdrica/limitiAziende";

            /*
            // L'utente non è autenticato, reindirizzamento sulla pagina di login
            if (!IsUserAuth()) return RedirectToPage("/auth/SignIn");

            // Imposta il token
            ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Token"]);

            // Creare il corpo della richiesta
            var requestBody = new
            {
                PartitaIvaAziendaIdrica = ApiReq.utente.PartitaIva,
                NuovoLimite = nuovoLimite,
                PartitaIvaAziendaAgricola = partitaIvaAzienda
            };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata PUT
            HttpResponseMessage response = await ApiReq.httpClient.PutAsync(urlTask, content);

            if (response.IsSuccessStatusCode)
            {
                // Imposta un messaggio di successo
                TempData["MessaggioLimite"] = "Modifica limite per l'azienda con P.Iva " + partitaIvaAzienda + " effettuata con successo!";
            }
            else
            {
                // Imposta un messaggio di errore
                TempData["MessaggioErroreLimite"] = "Errore durante la modifica. Riprova più tardi.";
            }
            */

            TempData["MessaggioErroreLimite"] = "Errore durante la modifica. Riprova più tardi.";

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





        // Metodo per simulare i dati di esempio
        private void SimulaDatiDiEsempio()
        {
            LimitiAcquistoPerAzienda = new List<LimiteAcquistoAzienda>
            {
                new LimiteAcquistoAzienda { PartitaIva = "12345678901", Nome = "Azienda1", LimiteAcquistoAziendale = 1000 },
                new LimiteAcquistoAzienda { PartitaIva = "98765432109", Nome = "Azienda2", LimiteAcquistoAziendale = 800 },
                new LimiteAcquistoAzienda { PartitaIva = "56789012345", Nome = "Azienda3", LimiteAcquistoAziendale = 1200 },
                // Aggiungi altri dati di esempio
            };
        }

    }
}
