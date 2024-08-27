using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace frontend.Pages.auth
{
    public class SignToFarmModel : PageModel
    {
        public UtenteAp? utenteAp {  get; set; }
        public bool isAlreadyRequest = false;

        public async Task<IActionResult> OnGet()
        {           
            try
            {
                // Controllo utente creato ed autenticato
                if (await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/DatiAccount");
                return Page();
            }
            catch (HttpRequestException ex)
            {                
                TempData["MessaggioErrore"] = $"Errore: {ex.Message}. Riprovare più tardi.";
                return RedirectToPage("/Error");
            }            
        }


        public async Task<IActionResult> OnPostIscriviti(string nome, string cognome, string email, string codiceFiscale, string partitaIva, string tipoAzienda)
        {
            string urlTask = ApiReq.urlGenerico + "/service/apply";

            // Controllo dei parametri in input
            if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(cognome) || string.IsNullOrEmpty(codiceFiscale) || string.IsNullOrEmpty(partitaIva) || string.IsNullOrEmpty(tipoAzienda))
            {
                TempData["MessaggioErrore"] = "Tutti i campi devono essere compilati.";
                return RedirectToPage();
            }

            if (tipoAzienda != "WSP" && tipoAzienda != "FAR")
            {
                TempData["MessaggioErrore"] = "Errore di compilazione del form.";
                return RedirectToPage();
            }
            
            try
            {
                // Imposta il token
                ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Token"]);

                // Creare il corpo della richiesta
                var requestBody = new
                {
                    given_name = nome,
                    family_name = cognome,
                    tax_code = codiceFiscale,
                    company_vat_number = partitaIva,
                    company_category = tipoAzienda,
                    email = email
                };
                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);
                Console.WriteLine(response.StatusCode);
                Console.WriteLine(await response.Content.ReadAsStringAsync());

                if (response.IsSuccessStatusCode)
                {
                    string tipoAziendaLabel = tipoAzienda == "WSP" ? "idrica" : "agricola"; 
                    TempData["Messaggio"] = $"""Richiesta d'iscrizione all'azienda {tipoAziendaLabel} con P.Iva "{partitaIva}" per {nome} {cognome} effettuata con successo!""";
                }
                else
                {
                    switch ((int)response.StatusCode)
                    {
                        case StatusCodes.Status400BadRequest:
                            TempData["MessaggioErrore"] = "Errore 400. Domanda di adesione per questo utente già eseguita.";
                            break;
                        case StatusCodes.Status401Unauthorized:
                            TempData["MessaggioErrore"] = "Errore 401. Non autorizzato.";
                            break;
                        default:
                            TempData["MessaggioErrore"] = "Errore: " + response.ReasonPhrase;
                            break;                           
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                TempData["MessaggioErrore"] = "Errore nella richiesta HTTP: " + ex.Message;
                return RedirectToPage("/Error");
            }

            return RedirectToPage();
        }



        public IActionResult OnPostAnnulla()
        {
            // Cancella il cookie del token
            Response.Cookies.Delete("Token");

            // Reindirizza alla pagina di accesso
            return RedirectToPage("/auth/SignIn");
        }

    }
}