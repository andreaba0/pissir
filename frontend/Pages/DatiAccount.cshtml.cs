using frontend.Models;
using Microsoft.AspNetCore.Authentication;
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
        public AziendaAgricolaModel? aziendaAgricola { get; set; }
        public AziendaIdricaModel? aziendaIdrica { get; set; }

        public async Task<IActionResult> OnGet()
        {
            /*
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                // Chiamata alle API per ottenere i dati
                ApiReq.utente = await ApiReq.GetUserDataFromApi(HttpContext);
                azienda = await ApiReq.GetAziendaDataFromApi(HttpContext);

                if (ApiReq.utente.Role == "WSP")
                    aziendaIdrica = await ApiReq.GetAziendaIdricaDataFromApi(HttpContext);
                else if (ApiReq.utente.Role == "FAR")
                    aziendaAgricola = await ApiReq.GetAziendaAgricolaDataFromApi(HttpContext);
                else
                    throw new Exception("Errore nel prelevare i dati dell'account.");
            }
            catch (Exception ex) 
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
            */

            // Simulazione dati          
            if (ApiReq.utente.Role == "WSP")
                aziendaIdrica = GetSimulatedAziendaIdricaData();
            else
                aziendaAgricola = GetSimulatedAziendaAgricolaData();
            
            // Continua con la generazione della pagina
            return Page();
        }

        public async Task<IActionResult> OnPostAggiornaDatiAzienda(string partitaIva, string nomeAzienda, string indirizzoAzienda, string telefonoAzienda, string emailAzienda, string categoria)
        {
            string urlTask = ApiReq.urlGenerico + "/company";

            /*
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
                    name = nomeAzienda,
                    address = indirizzo,
                    phone = telefono,
                    email = emailAzienda,
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
                    TempData["MessaggioErrore"] = "Errore durante l'aggiornamento dei dati dell'azienda. Riprova più tardi.";
                }
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
            */
            string datiAggiornati2 = $"Nome Azienda: {nomeAzienda}, Indirizzo: {indirizzoAzienda}, Telefono: {telefonoAzienda}, Email: {emailAzienda}";
            TempData["Messaggio"] = $"Aggiornamento dei dati dell'azienda {datiAggiornati2} effettuato con successo!"; TempData["MessaggioErrore"] = "Errore durante l'aggiornamento dei dati dell'azienda. Riprova più tardi.";
            TempData["MessaggioErrore"] = "Errore durante l'aggiornamento dei dati dell'azienda. Riprova più tardi.";

            return RedirectToPage();
        }



        // Funzione per effettuare il logout
        public async Task<IActionResult> OnPostLogout()
        {
            // Cancella il cookie del token
            Response.Cookies.Delete("Token");

            // Reindirizza alla pagina di accesso
            return RedirectToPage("/auth/SignIn");
        }






        // Simula i dati di un'azienda agricola
        private AziendaAgricolaModel GetSimulatedAziendaAgricolaData()
        {
            return new AziendaAgricolaModel
            {
                PartitaIva = "1234567890",
                Nome = "Azienda Agricola Rossi",
                Indirizzo = "Via delle Campagne, 123",
                Telefono = "0123456789",
                Email = "info@aziendaagricolarossi.com",
                Categoria = "FA"
            };
        }


        // Simula i dati di un'azienda idrica
        private AziendaIdricaModel GetSimulatedAziendaIdricaData()
        {   
            return new AziendaIdricaModel
            {
                PartitaIva = "9876543210",
                Nome = "Azienda Idrica Blu",
                Indirizzo = "Via dell'Acqua, 456",
                Telefono = "9876543210",
                Email = "info@aziendaidricablu.com",
                Categoria = "WA"
            };
        }




    }
}
