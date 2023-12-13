using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Text;

namespace frontend.Pages.auth
{
    public class SignFarmModel : PageModel
    {
        public async Task<IActionResult> OnGet()
        {
            // L'utente non è autenticato, reindirizzamento sulla pagina di login
            //if (!IsUserAuth()) return RedirectToPage("/auth/SignIn");

            return Page();
        }

        // Chiamata API per aggiunta azienda agricola
        public async Task<IActionResult> OnPostRegistraAzienda(string PartitaIva, string Nome, string Indirizzo, string Telefono, string Email)
        {
            string urlTask = ApiReq.urlGenerico + "registraAzienda/";

            /*
            // L'utente non è autenticato, reindirizzamento sulla pagina di login
            if (!IsUserAuth()) return RedirectToPage("/auth/SignIn");

            // Creare il corpo della richiesta
            var requestBody = new
            {
                PartitaIva = PartitaIva,
                Nome = Nome,
                Indirizzo = Indirizzo,
                Telefono = Telefono,
                Email = Email
            };

            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata POST per l'aggiunta dell'azienda agricola
            HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

            if (response.IsSuccessStatusCode)
            {
                // Imposta un messaggio di successo
                TempData["Messaggio"] = "Richiesta di registrazione dell'azienda con P.Iva " + PartitaIva + " effettuata con successo!";
                TempData["PartitaIva"] = PartitaIva;
                TempData["Nome"] = Nome;
                TempData["Indirizzo"] = Indirizzo;
                TempData["Telefono"] = Telefono;
                TempData["Email"] = Email;
            }
            else
            {
                // Imposta un messaggio di errore
                TempData["MessaggioErrore"] = "Errore durante la registrazione dell'azienda. Riprova più tardi.";
            }
            */
            
            TempData["Messaggio"] = "Richiesta di registrazione dell'azienda con P.Iva " + PartitaIva + " effettuata con successo!";
            TempData["PartitaIva"] = PartitaIva;
            TempData["Nome"] = Nome;
            TempData["Indirizzo"] = Indirizzo;
            TempData["Telefono"] = Telefono;
            TempData["Email"] = Email;

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
