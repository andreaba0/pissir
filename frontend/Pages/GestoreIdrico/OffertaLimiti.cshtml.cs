using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using frontend.Models;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;
using System.Globalization;

namespace frontend.Pages.GestoreIdrico
{
    public class OffertaLimitiModel : PageModel
    {
        public List<Offerta>? OfferteInserite { get; set; }
        public List<LimiteAcquistoAzienda>? LimitiAcquistoPerAzienda { get; set; }
        public float AcquaDisponibile { get; set; }
        public float LimiteGiornalieroVendita { get; set; }

        public async Task<IActionResult> OnGet()
        {
            /*
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                ApiReq.utente = await ApiReq.GetUserDataFromApi(HttpContext);
                OfferteInserite = await ApiReq.GetOfferteInserite(ApiReq.utente.PartitaIva, HttpContext);
                //LimiteGiornalieroVendita = await ApiReq.GetLimiteGiornaliero(ApiReq.utente.PartitaIva, HttpContext);
                LimitiAcquistoPerAzienda = await ApiReq.GetLimitiPerAziendaFromApi(HttpContext);
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
            */

            // Simula i dati di esempio
            SimulaDatiDiEsempio();

            return Page();
        }

        
        // Chiamata API per modificare l'offerta idrica
        public async Task<IActionResult> OnPostInserisciOfferta(string quantitaAcqua, string dataDisp, string prezzoAcqua)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaIdrica/offertaLimiti";

            // Controllo se le date sono nel formato corretto yyyy-MM-dd
            if (!DateTime.TryParseExact(dataDisp, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                TempData["MessaggioErrore"] = "Formato data non valido. Utilizzare il formato gg/mm/aaaa. Ricevuto: " + dataDisp;
                return RedirectToPage();
            }

            // Ottenere la data odierna
            DateTime today = DateTime.Now.Date;

            // Controllare se la data è posteriore a quella odierna
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

            /*
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                // Imposta il token
                ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Token"]);

                // Creare il corpo della richiesta
                var requestBody = new
                {
                    PartitaIva = ApiReq.utente.PartitaIva,
                    QuantitaAcqua = quantitaAcqua,
                    DataDisp = dataDisp,
                    PrezzoLitro = prezzoAcqua
                };
                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Esegue la chiamata PUT
                HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

                if (response.IsSuccessStatusCode)
                {
                    // Imposta un messaggio di successo
                    TempData["Messaggio"] = $"Inserimento offerta effettuato con successo! Quantità: {quantitaAcqua}L - Data disponibilità: {dataDisp} - Prezzo: {prezzoAcqua}€/L";
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
            */

            TempData["Messaggio"] = $"Inserimento offerta effettuato con successo! Quantità: {quantitaAcqua}L - Data disponibilità: {dataDisp} - Prezzo: {prezzoAcqua}€/L";
            TempData["MessaggioErrore"] = "Errore durante l'inserimento. Riprova più tardi.";

            return RedirectToPage();
        }

        // Chiamata API per modificare il limite di acquisto per le aziende agricole
        public async Task<IActionResult> OnPostModificaLimiteAziendale(string nuovoLimite, string partitaIvaAzienda)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaIdrica/limitiAziende";

            /*
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

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
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
            
            
            */

            TempData["MessaggioErroreLimite"] = "Errore durante la modifica. Riprova più tardi.";

            return RedirectToPage();
        }

        // Metodo per simulare i dati di esempio
        private void SimulaDatiDiEsempio()
        {
            OfferteInserite = new List<Offerta>
            {
                new Offerta { Id = "Offerta1", PartitaIva = "8989898989" , DataAnnuncio = "2024-02-10", PrezzoLitro = 15.9f, Quantita = 400.0f },
                new Offerta { Id = "Offerta2", PartitaIva = "8989898989" , DataAnnuncio = "2024-02-11", PrezzoLitro = 14.9f, Quantita = 300.0f },
                new Offerta { Id = "Offerta3", PartitaIva = "8989898989" , DataAnnuncio = "2024-02-12", PrezzoLitro = 13.9f, Quantita = 350.0f },
                new Offerta { Id = "Offerta4", PartitaIva = "8989898989" , DataAnnuncio = "2024-02-13", PrezzoLitro = 14.9f, Quantita = 200.0f }
            };

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
