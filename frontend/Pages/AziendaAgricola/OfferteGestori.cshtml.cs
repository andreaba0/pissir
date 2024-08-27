using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace frontend.Pages.AziendaAgricola
{
    public class OfferteGestoriModel : PageModel
    {
        public List<Offerta>? Offerte { get; set; }
        public List<Coltura>? Colture { get; set; }
        public float? LimiteAcquistoAzienda { get; set; }
        public List<AcquaStimata>? CampiAcquaStimata {  get; set; }

        public async Task<IActionResult> OnGet()
        {
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                // Controllo utente autorizzato
                if (ApiReq.utente.Role != "FA") { return RedirectToPage("/DatiAccount"); }

                // Simulazione dati
                Offerte = GetListaOfferte();
                Colture = GetListaColture();
                CampiAcquaStimata = GetAcquaSitmata();
                LimiteAcquistoAzienda = 500.0f;

                return Page();

                // Richiesta API
                string data = await ApiReq.GetDataFromApi(HttpContext, "/water/offer");
                Offerte = JsonConvert.DeserializeObject<List<Offerta>>(data);

                data = await ApiReq.GetDataFromApi(HttpContext, "/field");
                Colture = JsonConvert.DeserializeObject<List<Coltura>>(data);

                data = await ApiReq.GetDataFromApi(HttpContext, "/water/limit");
                LimiteAcquistoAzienda = JsonConvert.DeserializeObject<float>(data);

                foreach (Coltura campo in Colture)
                {
                    data = await ApiReq.GetDataFromApi(HttpContext, "/water/recommendation/"+campo.Id);
                    AcquaStimata new_item = JsonConvert.DeserializeObject<AcquaStimata>(data);
                    new_item.CampoId = campo.Id;
                    CampiAcquaStimata.Add(new_item);
                }

                //data = await ApiReq.GetDataFromApi(HttpContext, "/water/recommendation");
                //CampiAcquaStimata = JsonConvert.DeserializeObject<List<AcquaStimata>>(data);
            }
            catch (HttpRequestException ex)
            {
                string statusCode = ex.Message.ToString().ToLower();

                if (statusCode == "unauthorized")
                {
                    // Errore 401
                    TempData["MessaggioErrore"] = "Non sei autorizzato. Effettua prima la richiesta di accesso ai servizi.";
                    return RedirectToPage("/Error");
                }
                else
                {
                    TempData["MessaggioErrore"] = $"Errore: {ex.Message}. Riprovare più tardi.";
                    return RedirectToPage("/Error");
                }
            }
        }

        


        // Chiamata API per acquisto risorse idriche
        public async Task<IActionResult> OnPostOrdineAcquisto(string colturaId, string offertaId, string quantitaAcquisto)
        {
            string urlTask = ApiReq.urlGenerico + "/water/buy";

            // Estrai il colturaId dal formato "Id-TipoColtura-MetriQuadrati-TotaleStimato-TotaleRimanente"
            string[] colturaIdParts = colturaId.Split('-');
            if (colturaIdParts.Length >= 1)
            {
                colturaId = colturaIdParts[0];
            }
            // Nessuna coltura selezionata
            if(colturaId == "null")
            {
                TempData["MessaggioErrore"] = "Selezionare una coltura prima di acquistare le risorse.";
                return RedirectToPage();
            }

            /*
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");
                
                // Controllo utente autorizzato
                if (ApiReq.utente.Role!="FAR") { throw new Exception("Unauthorized"); }
                
                // Imposta il token
                ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Token"]);

                // Creare il corpo della richiesta
                var requestBody = new
                {
                    field_id = colturaId,
                    amount = quantitaAcquisto,
                    offer_id = offertaId,
                    date = DateTime.Now
                };
                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Esegue la chiamata POST
                HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

                if (response.IsSuccessStatusCode)
                {
                    // Imposta un messaggio di successo
                    TempData["Messaggio"] = $"Risorse idriche acquistate ( {quantitaAcquisto}L ) dall'offerta con ID: {offertaId} per il campo con ID: {colturaId}";
                }
                else
                {
                    // Imposta un messaggio di errore
                    TempData["MessaggioErrore"] = "Errore durante l'acquisto. Riprova pi� tardi.";
                }
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
            */

            TempData["Messaggio"] = $"Risorse idriche acquistate ( {quantitaAcquisto}L ) dall'offerta con ID: {offertaId} per il campo con ID: {colturaId}";
            TempData["MessaggioErrore"] = "Errore durante l'acquisto. Riprova più tardi.";

            
            return RedirectToPage();
        }




        // Simulazione dati
        private List<Offerta> GetListaOfferte()
        {
            // Simulazione di dati
            return new List<Offerta>
            {
                new Offerta { Id = "1", DataAnnuncio = "2023-11-03", PrezzoLitro = 1.5f, Quantita = 100 },
                new Offerta { Id = "2", DataAnnuncio = "2023-11-04", PrezzoLitro = 1.8f, Quantita = 150 },
                new Offerta { Id = "3", DataAnnuncio = "2023-11-05", PrezzoLitro = 2.0f, Quantita = 120 }
            };
        }

        // Simulazione dati
        private List<Coltura> GetListaColture()
        {
            // Simulazione di dati
            return new List<Coltura>
            {
                new Coltura { Id = "1", MetriQuadrati = 100, TipoColtura = "Grano", TipoIrrigazione = "Goccia" },
                new Coltura { Id = "2", MetriQuadrati = 200, TipoColtura = "Sorgo", TipoIrrigazione = "Goccia" },
                new Coltura { Id = "3", MetriQuadrati = 150, TipoColtura = "Mais", TipoIrrigazione = "Spruzzo" }
            };
        }

        private List<AcquaStimata> GetAcquaSitmata()
        {
            // Simulazione di dati
            return new List<AcquaStimata>
            {
                new AcquaStimata { CampoId = "1", TotaleStimato= 200.0f, TotaleRimanente=150.0f },
                new AcquaStimata { CampoId = "2", TotaleStimato= 400.0f, TotaleRimanente=300.0f },
                new AcquaStimata { CampoId = "3", TotaleStimato= 300.0f, TotaleRimanente=300.0f }
            };
        }

    }
}
