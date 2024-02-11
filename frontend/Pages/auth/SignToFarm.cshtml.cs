using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Buffers.Text;

namespace frontend.Pages.auth
{
    public class SignToFarmModel : PageModel
    {
        public UtenteAp utenteAp {  get; set; }

        /*private Task<IDictionary<string, string>> GetPayloadFromToken(string token)
        {
            string[] tokenParts = token.Split('.');
            string payload = tokenParts[1];
            string payloadJson = Encoding.UTF8.GetString(Base64Url.Decode(payload));
            return Task.FromResult(JsonConvert.DeserializeObject<IDictionary<string, string>>(payloadJson));
        }*/

        public async Task<IActionResult> OnGet()
        {
            
            try
            {
                
                // Controllo utente autenticato
                if (!HttpContext.Request.Cookies.TryGetValue("Token", out string token)) return RedirectToPage("/auth/SignIn");

                //IDictionary<string, string> payload = await GetPayloadFromToken(token);
                IDictionary<string, string> payload = new Dictionary<string, string>();

                utenteAp = new UtenteAp
                {
                    Id = (payload.ContainsKey("sub")) ? payload["sub"] : null,
                    Nome = (payload.ContainsKey("given_name")) ? payload["given_name"] : null,
                    Cognome = (payload.ContainsKey("family_name")) ? payload["family_name"] : null,
                    Email = (payload.ContainsKey("email")) ? payload["email"] : null,
                    CodiceFiscale = null
                };

                return Page();

                // Chiamata alle API per ottenere i dati
                utenteAp = await ApiReq.GetUserDataApplicationFromApi(HttpContext);
                
                if (utenteAp == null)
                {
                    ViewData["ErrorMessage"] = "Si � verificato un errore durante l'accesso. ";
                    return RedirectToPage("/Error");
                }
                
                DatiTest();


                //TODO application status : accept, reject, pending
                // Controllo per ogni campo di UtenteAp
                if (!(string.IsNullOrEmpty(utenteAp.Id) ||
                    string.IsNullOrEmpty(utenteAp.PartitaIva) ||
                    string.IsNullOrEmpty(utenteAp.Nome) ||
                    string.IsNullOrEmpty(utenteAp.Cognome) ||
                    string.IsNullOrEmpty(utenteAp.Email) ||
                    string.IsNullOrEmpty(utenteAp.CodiceFiscale) ||
                    string.IsNullOrEmpty(utenteAp.TipoAzienda)))
                {
                    return RedirectToPage("/auth/AuthPeriod");
                }
            }
            catch (HttpRequestException ex)
            {
                switch (ex.Message.ToString().ToLower())
                {
                    case "badrequest":
                        TempData["MessaggioErrore"] = "Errore 400. Utente gi� accettato nel sistema.";
                        break;
                    case "unauthorized":
                        TempData["MessaggioErrore"] = "Errore 401. Non autorizzato.";
                        break;
                    case "notfound":
                        TempData["MessaggioErrore"] = "Errore 404. Richiesta d'adesione non trovata.";
                        break;
                    default:
                        TempData["MessaggioErrore"] = $"Errore: {ex.Message}. Riprovare pi� tardi.";
                        break;
                }

                return RedirectToPage("/Error");
            }
            

            DatiTest();
            
            return Page();
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
                // Controllo utente autenticato
                //if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

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
                    TempData["Messaggio"] = "Richiesta d'iscrizione all'azienda P.Iva " + partitaIva + nome + cognome + codiceFiscale + tipoAzienda + " effettuata con successo!";
                }
                else
                {
                    switch ((int)response.StatusCode)
                    {
                        case StatusCodes.Status400BadRequest:
                            TempData["MessaggioErrore"] = "Errore 400. Domanda di adesione per questo utente gi� eseguita.";
                            break;
                        case StatusCodes.Status401Unauthorized:
                            TempData["MessaggioErrore"] = "Errore 401. Non autorizzato.";
                            break;
                        default:
                            TempData["MessaggioErrore"] = "Errore: " + response.ReasonPhrase;
                            break;                           
                    }
                    //return RedirectToPage("/Error");
                }
            }
            catch (HttpRequestException ex)
            {
                TempData["MessaggioErrore"] = "Errore nella richiesta HTTP: " + ex.Message;
                return RedirectToPage("/Error");
            }
            

            //TempData["Messaggio"] = "Richiesta d'iscrizione all'azienda P.Iva " + partitaIva + nome + cognome + codiceFiscale + tipoAzienda + " effettuata con successo!";
            //TempData["MessaggioErrore"] = "Errore durante la richiesta. Riprova pi� tardi.";

            return RedirectToPage();
        }



        // TEST
        private void DatiTest()
        {
            utenteAp = new UtenteAp
            {
                CodiceFiscale = "ABC123XYZ4567890",
                //PartitaIva = "1234567890",
                //TipoAzienda = "FAR",
                //TipoAzienda = "WSP",
                Id="1",
                Nome = "Mario",
                Cognome = "Rossi",
                Email = "mariorossi@mail.mail"
            };
        }



    }
}
