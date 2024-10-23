using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace frontend.Pages
{
    public class DatiAccountModel : PageModel
    {
        public Azienda azienda { get; set; }
        
        public async Task<IActionResult> OnGet()
        {
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                // Richiesta API
                string data = await ApiReq.GetDataFromAuth(HttpContext, "/company", true);
                azienda = JsonConvert.DeserializeObject<Azienda>(data);

                return Page();

            }
            catch (Exception ex) 
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
        }

        public async Task<IActionResult> OnPostAggiornaDatiAzienda(string partitaIva, string nomeAzienda, string indirizzoAzienda, string telefonoAzienda, string emailAzienda, string categoria)
        {
            string urlTask = ApiReq.authUrlGenerico + "/company";

            
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                // Imposta il token
                ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Token"]);

                // Creare il corpo della richiesta con i dati dell'azienda
                var requestBody = new
                {
                    vat_number = partitaIva,
                    company_name = nomeAzienda,
                    working_address = indirizzoAzienda,
                    working_phone_number = telefonoAzienda,
                    working_email_address = emailAzienda,
                    industry_sector = categoria
                };

                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Esegue la chiamata PUT per l'aggiornamento dei dati dell'azienda
                HttpResponseMessage response = await ApiReq.httpClient.PatchAsync(urlTask, content);

                if (response.IsSuccessStatusCode)
                {
                    // Imposta un messaggio di successo
                    string datiAggiornati = $"Nome Azienda: {nomeAzienda}, Indirizzo: {indirizzoAzienda}, Telefono: {telefonoAzienda}, Email: {emailAzienda}";
                    TempData["Messaggio"] = $"Aggiornamento dei dati dell'azienda {datiAggiornati} effettuato con successo!";
                }
                else
                {
                    // Imposta un messaggio di errore
                    TempData["MessaggioErrore"] = "Errore durante l'aggiornamento dei dati dell'azienda. Riprova pi� tardi.";
                }
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
            
            //string datiAggiornati2 = $"Nome Azienda: {nomeAzienda}, Indirizzo: {indirizzoAzienda}, Telefono: {telefonoAzienda}, Email: {emailAzienda}";
            //TempData["Messaggio"] = $"Aggiornamento dei dati dell'azienda {datiAggiornati2} effettuato con successo!"; TempData["MessaggioErrore"] = "Errore durante l'aggiornamento dei dati dell'azienda. Riprova pi� tardi.";
            //TempData["MessaggioErrore"] = "Errore durante l'aggiornamento dei dati dell'azienda. Riprova pi� tardi.";

            return RedirectToPage();
        }

        // Funzione per effettuare il logout
        public IActionResult OnPostLogout()
        {
            // Cancella il cookie del token
            Response.Cookies.Delete("Token");

            // Reindirizza alla pagina di accesso
            return RedirectToPage("/auth/SignIn");
        }

    }
}
