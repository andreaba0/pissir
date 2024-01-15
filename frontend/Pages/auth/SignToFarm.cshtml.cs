using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;

namespace frontend.Pages.auth
{
    public class SignToFarmModel : PageModel
    {
        public string CodiceFiscale { get; set; }
        public string NomeUtente { get; set; }
        public string CognomeUtente { get; set; }

        public void OnGet()
        {
            // Recupera i dati dell'utente dal contesto di autenticazione o da dove sono memorizzati
            // Ad esempio, potresti avere questi dati in un cookie o in una sessione
            //CodiceFiscale = "ABC123XYZ4567890";
            NomeUtente = "Mario";
            CognomeUtente = "Rossi";

        }

        public async Task<IActionResult> OnPostIscriviti(string nome, string cognome, string codiceFiscale, string partitaIva)
        {
            string urlTask = ApiReq.urlGenerico + "registrazione/";

            /*
            // L'utente non � autenticato, reindirizzamento sulla pagina di login
            if (!IsUserAuth()) return RedirectToPage("/auth/SignIn");

            // Imposta il token
            ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Token"]);

            // Creare il corpo della richiesta
            var requestBody = new
            {
                Nome = nome,
                Cognome = cognome,
                CodiceFiscale = codiceFiscale,
                PartitaIva = partitaIva
            };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata POST per l'aggiunta della coltura
            HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

            if (response.IsSuccessStatusCode)
            {
                // Imposta un messaggio di successo
                TempData["Messaggio"] = "Richiesta d'iscrizione all'azienda P.Iva "+ partitaIva + nome +cognome +codiceFiscale +" effettuata con successo!";
            }
            else
            {
                // Imposta un messaggio di errore
                TempData["MessaggioErrore"] = "Errore durante la richiesta. Riprova pi� tardi.";
            }
            */
            //TempData["Messaggio"] = "Richiesta d'iscrizione all'azienda P.Iva " + partitaIva + nome + cognome + codiceFiscale + " effettuata con successo!";
            TempData["MessaggioErrore"] = "Errore durante la richiesta. Riprova pi� tardi.";

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
    }
}
