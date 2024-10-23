using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using frontend.Models;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;

namespace frontend.Pages.GestoreIdrico
{
    public class OffertaLimitiModel : PageModel
    {
        public List<Offerta>? OfferteInserite { get; set; }
        public List<LimiteAcquistoAzienda>? LimitiAcquistoPerAzienda { get; set; }
        public float AcquaDisponibile { get; set; }

        public async Task<IActionResult> OnGet()
        {   
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                // Controllo utente autorizzato
                if (ApiReq.utente.Role!="WA") { throw new Exception("Unauthorized"); }

                // Simula i dati di esempio
                SimulaDatiDiEsempio();

                return Page();

                // Richiesta API
                string data = await ApiReq.GetDataFromApi(HttpContext, "/water/offer", true, true);
                OfferteInserite = JsonConvert.DeserializeObject<List<Offerta>>(data);

                data = await ApiReq.GetDataFromApi(HttpContext, "/water/limit/all", true, true);
                LimitiAcquistoPerAzienda = JsonConvert.DeserializeObject<List<LimiteAcquistoAzienda>>(data);
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
        }

        
        // Chiamata API per inserire l'offerta idrica
        public async Task<IActionResult> OnPostInserisciOfferta(string quantitaAcqua, string dataDisp, string prezzoAcqua)
        {
            string urlTask = ApiReq.apiUrlGenerico + "/water/offer";

            // Controllo se le date sono nel formato corretto yyyy-MM-dd
            if (!DateTime.TryParseExact(dataDisp, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                TempData["MessaggioErrore"] = "Formato data non valido. Utilizzare il formato gg/mm/aaaa. Ricevuto: " + dataDisp;
                return RedirectToPage();
            }

            // Ottenere la data odierna
            DateTime today = DateTime.Now.Date;

            // Controllare se la data � posteriore a quella odierna
            if (date < today)
            {
                TempData["MessaggioErrore"] = "La data di inizio non può essere posteriore a oggi.";
                return RedirectToPage();
            }

            if (float.Parse(quantitaAcqua) <= 0.0f)
            {
                TempData["MessaggioErrore"] = "Quantità acqua erroneamente impostata.";
                return RedirectToPage();
            }

            if (float.Parse(prezzoAcqua) <= 0.0f)
            {
                TempData["MessaggioErrore"] = "Quantità acqua erroneamente impostata.";
                return RedirectToPage();
            }

            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                // Controllo utente autorizzato
                if (ApiReq.utente.Role!="WA") { throw new Exception("Unauthorized"); }    

                // Imposta il token
                await ApiReq.GetApiToken(HttpContext);
                ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["ApiToken"]);

                // Creare il corpo della richiesta
                var requestBody = new
                {
                    amount = quantitaAcqua,
                    price = prezzoAcqua,
                    date = dataDisp
                };
                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Esegue la chiamata PUT
                HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

                if (response.IsSuccessStatusCode)
                {
                    // Imposta un messaggio di successo
                    TempData["Messaggio"] = $"Inserimento offerta effettuato con successo! Quantità: {quantitaAcqua}L - Data disponibilità: {dataDisp} - Prezzo: {prezzoAcqua}�/L";
                }
                else
                {
                    // Imposta un messaggio di errore
                    TempData["MessaggioErrore"] = "Errore durante l'inserimento. Riprova più tardi.";
                }
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }

            //TempData["Messaggio"] = $"Inserimento offerta effettuato con successo! Quantit�: {quantitaAcqua}L - Data disponibilit�: {dataDisp} - Prezzo: {prezzoAcqua}�/L";
            //TempData["MessaggioErrore"] = "Errore durante l'inserimento. Riprova pi� tardi.";

            return RedirectToPage();
        }

        // Chiamata API per la modifica dell'offerta idrica
        public async Task<IActionResult> OnPostModificaOfferta(string nuovaQuantita, string offertaId)
        {
            string urlTask = ApiReq.apiUrlGenerico + $"/water/offer/{offertaId}";

            if (float.Parse(nuovaQuantita) <= 0.0f)
            {
                TempData["MessaggioErrore"] = "Quantità acqua erroneamente impostata.";
                return RedirectToPage();
            }
 
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");
                
                // Controllo utente autorizzato
                if (ApiReq.utente.Role!="WA") { throw new Exception("Unauthorized"); }

                // Imposta il token
                await ApiReq.GetApiToken(HttpContext);
                ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["ApiToken"]);

                // Creare il corpo della richiesta
                var requestBody = new
                {
                    update_amount_to = nuovaQuantita
                };
                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Esegue la chiamata PATCH
                HttpResponseMessage response = await ApiReq.httpClient.PatchAsync(urlTask, content);

                if (response.IsSuccessStatusCode)
                {
                    // Imposta un messaggio di successo
                    TempData["Messaggio"] = $"Modifica dell'offerta con ID: {offertaId} effettuata con successo! Quantità: {nuovaQuantita}L.";
                }
                else
                {
                    // Imposta un messaggio di errore
                    TempData["MessaggioErrore"] = "Errore durante l'inserimento. Riprova più tardi.";
                }
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }

            //TempData["Messaggio"] = $"Modifica dell'offerta con ID: {offertaId} effettuata con successo! Quantit�: {nuovaQuantita}L.";
            //TempData["MessaggioErrore"] = "Errore durante l'inserimento. Riprova pi� tardi.";

            return RedirectToPage();
        }

        // Chiamata API per eliminazione coltura
        public async Task<IActionResult> OnPostEliminaOfferta(string offertaId)
        {
            string urlTask = ApiReq.apiUrlGenerico + $"/water/offer/{offertaId}";

            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                // Controllo utente autorizzato
                if (ApiReq.utente.Role!="WA") { throw new Exception("Unauthorized"); }

                // Imposta il token
                await ApiReq.GetApiToken(HttpContext);
                ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["ApiToken"]);

                // Esegue la chiamata DELETE
                HttpResponseMessage response = await ApiReq.httpClient.DeleteAsync(urlTask);

                if (response.IsSuccessStatusCode)
                {
                    // Imposta un messaggio di successo
                    TempData["Messaggio"] = $"Eliminazione dell'offerta con ID: {offertaId} effettuata con successo!";
                }
                else
                {
                    // Imposta un messaggio di errore
                    TempData["MessaggioErrore"] = "Errore durante l'eliminazione dell'offerta. Riprova più tardi.";
                }
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }

            //TempData["Messaggio"] = $"Eliminazione dell'offerta con ID: {offertaId} effettuata con successo!";
            //TempData["MessaggioErrore"] = "Errore durante l'eliminazione dell'offerta. Riprova pi� tardi.";

            return RedirectToPage();
        }



        //// Chiamata API per modificare il limite di vendita aziendale giornaliero
        //public async Task<IActionResult> OnPostImpostaLimiteGiornaliero(string limiteAcqua, string dataInizio, string dataFine)
        //{
        //    string urlTask = ApiReq.urlGenerico + "/water/limit";

        //    // Controllo se le date sono nel formato corretto yyyy-MM-dd
        //    if (!DateTime.TryParseExact(dataInizio, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate) ||
        //        !DateTime.TryParseExact(dataFine, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
        //    {
        //        TempData["MessaggioErrore"] = "Formato data non valido. Utilizzare il formato gg/mm/aaaa. Ricevuto: " + dataInizio + " - " + dataFine;
        //        return RedirectToPage();
        //    }

        //    // Ottenere la data odierna
        //    DateTime today = DateTime.Now.Date;

        //    // Controllare se la data di inizio � posteriore a quella odierna
        //    if (startDate < today)
        //    {
        //        TempData["MessaggioErrore"] = "La data di inizio non pu� essere posteriore a oggi.";
        //        return RedirectToPage();
        //    }

        //    // Controllare se la data di fine � precedente alla data di inizio
        //    if (endDate < startDate)
        //    {
        //        TempData["MessaggioErrore"] = "La data di fine non pu� essere precedente alla data di inizio.";
        //        return RedirectToPage();
        //    }


        //    try
        //    {
        //        // Controllo utente autenticato
        //        if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

        //        // Controllo utente autorizzato
        //        if (ApiReq.utente.Role != "WA") { throw new Exception("Unauthorized"); }

        //        // Imposta il token
        //        ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Token"]);

        //        // Creare il corpo della richiesta
        //        var requestBody = new
        //        {
        //            limit = limiteAcqua,
        //            start_date = dataInizio,
        //            end_date = dataFine
        //        };
        //        var jsonRequest = JsonConvert.SerializeObject(requestBody);
        //        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        //        // Esegue la chiamata PUT
        //        HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            // Imposta un messaggio di successo
        //            TempData["MessaggioLimite"] = $"Modifica limite di vendita giornaliero a {limiteAcqua} per il periodo {dataInizio} - {dataFine} effettuata con successo!";
        //        }
        //        else
        //        {
        //            // Imposta un messaggio di errore
        //            TempData["MessaggioErroreLimite"] = "Errore durante la modifica. Riprova pi� tardi.";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["MessaggioErrore"] = ex.Message;
        //        return RedirectToPage("/Error");
        //    }


        //    TempData["MessaggioLimite"] = $"Modifica limite di vendita giornaliero a {limiteAcqua} per il periodo {dataInizio} - {dataFine} effettuata con successo!";
        //    TempData["MessaggioErroreLimite"] = "Errore durante la modifica. Riprova pi� tardi.";

        //    return RedirectToPage();
        //}



        // Chiamata API per modificare il limite di acquisto per le aziende agricole
        public async Task<IActionResult> OnPostModificaLimiteAziendale(string nuovoLimite, string partitaIvaAzienda, string dataInizio, string dataFine)
        {
            string urlTask = ApiReq.apiUrlGenerico + "/water/limit";

            // Controllo se le date sono nel formato corretto yyyy-MM-dd
            if (!DateTime.TryParseExact(dataInizio, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate) ||
                !DateTime.TryParseExact(dataFine, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
            {
                TempData["MessaggioErrore"] = "Formato data non valido. Utilizzare il formato gg/mm/aaaa. Ricevuto: " + dataInizio + " - " + dataFine;
                return RedirectToPage();
            }

            // Ottenere la data odierna
            DateTime today = DateTime.Now.Date;

            // Controllare se la data di inizio � posteriore a quella odierna
            if (startDate < today)
            {
                TempData["MessaggioErrore"] = "La data di inizio non può essere posteriore a oggi.";
                return RedirectToPage();
            }

            // Controllare se la data di fine � precedente alla data di inizio
            if (endDate < startDate)
            {
                TempData["MessaggioErrore"] = "La data di fine non può essere precedente alla data di inizio.";
                return RedirectToPage();
            }

            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                // Controllo utente autorizzato
                if (ApiReq.utente.Role!="WA") { throw new Exception("Unauthorized"); }    

                // Imposta il token
                await ApiReq.GetApiToken(HttpContext);
                ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["ApiToken"]);

                // Creare il corpo della richiesta
                var requestBody = new
                {
                    vat_number = partitaIvaAzienda,
                    limit = nuovoLimite,
                    start_date = startDate, 
                    end_date = endDate
                };
                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Esegue la chiamata
                HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

                if (response.IsSuccessStatusCode)
                {
                    // Imposta un messaggio di successo
                    TempData["MessaggioLimite"] = $"Modifica limite per l'azienda con P.Iva {partitaIvaAzienda} a {nuovoLimite} per il periodo {dataInizio} - {dataFine} effettuata con successo!";
                }
                else
                {
                    // Imposta un messaggio di errore
                    TempData["MessaggioErroreLimite"] = "Errore durante la modifica. Riprova pi� tardi.";
                }
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }

            //TempData["MessaggioLimite"] = $"Modifica limite per l'azienda con P.Iva {partitaIvaAzienda} a {nuovoLimite} per il periodo {dataInizio} - {dataFine} effettuata con successo!";
            //TempData["MessaggioErroreLimite"] = "Errore durante la modifica. Riprova più tardi.";

            return RedirectToPage();
        }




        // Metodo per simulare i dati di esempio
        private void SimulaDatiDiEsempio()
        {
            OfferteInserite = new List<Offerta>
            {
                new Offerta { Id = "Offerta1", DataAnnuncio = "2024-02-10", PrezzoLitro = 15.9f, Quantita = 400.0f },
                new Offerta { Id = "Offerta2", DataAnnuncio = "2024-02-11", PrezzoLitro = 14.9f, Quantita = 300.0f },
                new Offerta { Id = "Offerta3", DataAnnuncio = "2024-02-12", PrezzoLitro = 13.9f, Quantita = 350.0f },
                new Offerta { Id = "Offerta4", DataAnnuncio = "2024-02-13", PrezzoLitro = 14.9f, Quantita = 200.0f }
            };

            LimitiAcquistoPerAzienda = new List<LimiteAcquistoAzienda>
            {
                new LimiteAcquistoAzienda { PartitaIva = "12345678901", LimiteAcquistoAziendale = 1000, DataInizio = "2024-02-10", DataFine = "2024-03-10" },
                new LimiteAcquistoAzienda { PartitaIva = "98765432109", LimiteAcquistoAziendale = 800, DataInizio = "2024-02-10", DataFine = "2024-03-10" },
                new LimiteAcquistoAzienda { PartitaIva = "56789012345", LimiteAcquistoAziendale = 1200, DataInizio = "2024-02-12", DataFine = "2025-10-20" },
                new LimiteAcquistoAzienda { PartitaIva = "33389012345", LimiteAcquistoAziendale = 1500, DataInizio = "2024-03-12", DataFine = "2024-04-20" },

            };
        }

    }
}
