using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Globalization;

namespace frontend.Pages.AziendaAgricola
{
    public class AuthPeriodModel : PageModel
    {
        public List<UtentePeriodo>? requestList { get; set; }

        public async Task<IActionResult> OnGet()
        {
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                if (ApiReq.utente == null)
                {
                    TempData["MessaggioErrore"] = "Si è verificato un errore durante l'accesso. ";
                    return RedirectToPage("/Error");
                }

                if (ApiReq.utente.Role == "FA")
                {
                    return Page();
                }
                else if (ApiReq.utente.Role == "WA")
                {
                    return RedirectToPage("/DatiAccount");
                }
                else
                {
                    TempData["MessaggioErrore"] = "Non sei autorizzato a visualizzare questa pagina. ";
                    return RedirectToPage("/Error");
                }

            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
        }


        public async Task<IActionResult> OnPostInviaRichiesta(string dataInizio, string dataFine)
        {
            string urlTask = ApiReq.urlGenerico + "/apiaccess";

            // Controllo se le date sono nel formato corretto yyyy-MM-dd
            if (!DateTime.TryParseExact(dataInizio, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate) ||
                !DateTime.TryParseExact(dataFine, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
            {
                TempData["MessaggioErrore"] = "Formato data non valido. Utilizzare il formato gg/mm/aaaa. Ricevuto: " + dataInizio + " - " + dataFine;
                return RedirectToPage();
            }

            // Ottenere la data odierna
            DateTime today = DateTime.Now.Date;

            // Controllare se la data di inizio è posteriore a quella odierna
            if (startDate < today)
            {
                TempData["MessaggioErrore"] = "La data di inizio non può essere posteriore a oggi.";
                return RedirectToPage();
            }

            // Controllare se la data di fine è precedente alla data di inizio
            if (endDate < startDate)
            {
                TempData["MessaggioErrore"] = "La data di fine non può essere precedente alla data di inizio.";
                return RedirectToPage();
            }


            try
            {

                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                // Imposta il token
                ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Token"]);

                // Creare il corpo della richiesta
                var requestBody = new
                {
                    date_start = dataInizio,
                    date_end = dataFine
                };
                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Esegue la chiamata POST per l'aggiunta della coltura
                HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

                if (response.IsSuccessStatusCode)
                {
                    // Imposta un messaggio di successo
                    TempData["Messaggio"] = "Richiesta periodo di accesso al sistema da " + dataInizio + " a " + dataFine + " effettuata con successo!";
                }
                else
                {
                    // Imposta un messaggio di errore
                    TempData["MessaggioErrore"] = "Errore durante la richiesta. Riprova più tardi.";
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }

        }

    }
}
