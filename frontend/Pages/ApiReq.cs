using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace frontend.Pages
{
    public static class ApiReq
    {
        // Variabili comuni alle classi
        public static readonly string urlGenerico = "http://tuo-api-url.com/api/";
        public static readonly HttpClient httpClient = new();
        public static Utente? utente { get; set; }

        //Costruttore statico con utente statico
        static ApiReq()
        {
            utente = GetSimulatedUserData();
        }

        // Simula i dati di un utente
        private static Utente GetSimulatedUserData()
        {
            return new Utente
            {
                CodiceFiscale = "ABC123XYZ4567890",
                Nome = "Mario",
                Cognome = "Rossi",
                Role = "WSP" //WSP / FAR
            };
            
        }

        // Metodi richieste API

        // Richiesta dati utente
        public static async Task<Utente> GetUserDataFromApi(HttpContext context)
        {
            // Stringa interpolata
            string urlTask = $"{urlGenerico}user/?user_info=tax_code+name+surname+role";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["AccessToken"]);           
            
            // Esegue la chiamata
            HttpResponseMessage response = await httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                Utente? userData = JsonConvert.DeserializeObject<Utente>(responseData);
                if (userData != null)
                    return userData;
                else
                    throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode} - userData = null");
            }
            else
            {
                throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode}");
            }
            
        }

        // Richiesta dati azienda idrica
        public static async Task<AziendaIdricaModel> GetAziendaIdricaDataFromApi(HttpContext context)
        {
            string urlTask = $"{urlGenerico}aziendaIdrica/?azienda_info=vat_number+name+address+phone_number+email+industry_sector+daily_limit";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["AccessToken"]);

            // Esegue la chiamata
            HttpResponseMessage response = await ApiReq.httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                AziendaIdricaModel? aziendaData = JsonConvert.DeserializeObject<AziendaIdricaModel>(responseData);
                if (aziendaData != null)
                    return aziendaData;
                else
                    throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode} - aziendaData = null");
            }
            else
            {
                throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode}");
            }
        }


        // Richiesta dati azienda agricola
        public static async Task<AziendaAgricolaModel> GetAziendaAgricolaDataFromApi(HttpContext context)
        {
            string urlTask = $"{urlGenerico}aziendaAgricola/?azienda_info=vat_number+name+address+phone_number+email+industry_sector+buy_daily_limit";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["AccessToken"]);

            // Esegue la chiamata
            HttpResponseMessage response = await ApiReq.httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                AziendaAgricolaModel? aziendaData = JsonConvert.DeserializeObject<AziendaAgricolaModel>(responseData);
                if (aziendaData != null)
                    return aziendaData;
                else
                    throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode} - aziendaData = null");
            }
            else
            {
                throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode}");
            }
        }




        // Metodi Azienda Agricola -----------------------------------------------

        // Richiesta dati sulle colture possedute dall'azienda
        public static async Task<List<Coltura>> GetColtureAziendaFromApi(string partitaIva, HttpContext context)
        {
            string urlTask = $"{urlGenerico}aziendaAgricola/colture/?PartitaIva={Uri.EscapeDataString(partitaIva)}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["AccessToken"]);

            // Esegue la chiamata
            HttpResponseMessage response = await httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<Coltura>? listaColture = JsonConvert.DeserializeObject<List<Coltura>>(responseData);
                if (listaColture != null)
                    return listaColture;
                else
                    throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode} - listaColture = null");
            }
            else
            {
                throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode}");
            }
        }

        // Richiesta dati sulla lista di offerte delle aziende idriche
        public static async Task<List<Offerta>> GetOfferteIdricheFromApi(HttpContext context)
        {
            string urlTask = $"{urlGenerico}aziendaAgricola/offerteIdriche";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["AccessToken"]);

            // Esegue la chiamata
            HttpResponseMessage response = await ApiReq.httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<Offerta>? listaOfferte = JsonConvert.DeserializeObject<List<Offerta>>(responseData);
                if (listaOfferte != null)
                    return listaOfferte;
                else
                    throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode} - listaOfferte = null");
            }
            else
            {
                throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode}");
            }
        }


        // Richiesta dati dello storico ordini d'acqua
        public static async Task<List<OrdineAcquisto>> GetStoricoOrdiniFromApi(string partitaIva, HttpContext context)
        {
            string urlTask = $"{urlGenerico}aziendaAgricola/ordini/?PartitaIva={Uri.EscapeDataString(partitaIva)}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["AccessToken"]);

            // Esegue la chiamata
            HttpResponseMessage response = await ApiReq.httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<OrdineAcquisto>? listaOrdini = JsonConvert.DeserializeObject<List<OrdineAcquisto>>(responseData);
                if (listaOrdini != null)
                    return listaOrdini;
                else
                    throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode} - listaOrdini = null");
            }
            else
            {
                throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode}");
            }
        }



        // Richiesta dati sui consumi delle colture possedute dall'azienda
        public static async Task<List<ConsumoAziendaleCampo>> GetStoricoConsumiFromApi(string partitaIva, HttpContext context)
        {
            string urlTask = $"{urlGenerico}aziendaAgricola/consumiColture/?PartitaIva={Uri.EscapeDataString(partitaIva)}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["AccessToken"]);

            // Esegue la chiamata
            HttpResponseMessage response = await ApiReq.httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<ConsumoAziendaleCampo>? listaConsumi = JsonConvert.DeserializeObject<List<ConsumoAziendaleCampo>>(responseData);
                if (listaConsumi != null)
                    return listaConsumi;
                else
                    throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode} - listaConsumi = null");
            }
            else
            {
                throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode}");
            }
        }



        // Richiesta dati storico sensori di umidità
        public static async Task<List<SensoreUmiditaLog>> GetSensoriUmiditaFromApi(string partitaIva, HttpContext context)
        {
            string urlTask = $"{urlGenerico}aziendaAgricola/storicoSensoriUmidita/?PartitaIva={Uri.EscapeDataString(partitaIva)}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["AccessToken"]);

            // Esegue la chiamata 
            HttpResponseMessage response = await ApiReq.httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<SensoreUmiditaLog>? lista = JsonConvert.DeserializeObject<List<SensoreUmiditaLog>>(responseData);
                if (lista != null)
                    return lista;
                else
                    throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode} - lista = null");
            }
            else
            {
                throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode}");
            }
        }

        // Richiesta dati storico sensori di temperatura
        public static async Task<List<SensoreTemperaturaLog>> GetSensoriTemperaturaFromApi(string partitaIva, HttpContext context)
        {
            string urlTask = $"{urlGenerico}aziendaAgricola/storicoSensoriTemperatura/?PartitaIva={Uri.EscapeDataString(partitaIva)}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["AccessToken"]);

            // Esegue la chiamata
            HttpResponseMessage response = await ApiReq.httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<SensoreTemperaturaLog>? lista = JsonConvert.DeserializeObject<List<SensoreTemperaturaLog>>(responseData);
                if (lista != null)
                    return lista;
                else
                    throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode} - lista = null");
            }
            else
            {
                throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode}");
            }
        }


        // Richiesta dati storico attuatori
        public static async Task<List<AttuatoreLog>> GetAttuatoriFromApi(string partitaIva, HttpContext context)
        {
            string urlTask = $"{urlGenerico}aziendaAgricola/storicoAttuatori/?PartitaIva={Uri.EscapeDataString(partitaIva)}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["AccessToken"]);

            // Esegue la chiamata
            HttpResponseMessage response = await ApiReq.httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<AttuatoreLog>? lista = JsonConvert.DeserializeObject<List<AttuatoreLog>>(responseData);
                if (lista != null)
                    return lista;
                else
                    throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode} - lista = null");
            }
            else
            {
                throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode}");
            }
        }







        // Metodi Azienda Idrica ---------------------------------------------------------

        // Richiesta dati per limite giornaliero
        public static async Task<float> GetLimiteGiornaliero(string partitaIva, HttpContext context)
        {
            string urlTask = $"{urlGenerico}aziendaIdrica/limiteGiornaliero/?PartitaIva={Uri.EscapeDataString(partitaIva)}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["AccessToken"]);

            // Esegue la chiamata POST
            HttpResponseMessage response = await ApiReq.httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                float limite = JsonConvert.DeserializeObject<float>(responseData);
                if (responseData!=null)
                    return limite;
                else
                    throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode} - limite = null");
            }
            else
            {
                throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode}");
            }
        }


        // Richiesta dati acqua disponibile
        public static async Task<float> GetAcquaDisponibile(string partitaIva, HttpContext context)
        {
            string urlTask = $"{urlGenerico}aziendaIdrica/acquaDisponibile/?PartitaIva={Uri.EscapeDataString(partitaIva)}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["AccessToken"]);

            // Esegue la chiamata POST
            HttpResponseMessage response = await ApiReq.httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                float acqua = JsonConvert.DeserializeObject<float>(responseData);
                if (responseData != null)
                    return acqua;
                else
                    throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode} - acqua = null");
            }
            else
            {
                throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode}");
            }
        }

        // Richiesta dati sulle richieste di adesione per gli utenti
        public static async Task<List<Utente>> GetRichiesteUtentiFromApi(HttpContext context)
        {
            string urlTask = urlGenerico + "aziendaIdrica/richiesteUtenti/";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["AccessToken"]);

            // Esegue la chiamata POST
            HttpResponseMessage response = await httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<Utente>? listaUtenti = JsonConvert.DeserializeObject<List<Utente>>(responseData);
                if (listaUtenti != null)
                    return listaUtenti;
                else
                    throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode} - listaUtenti = null");
            }
            else
            {
                throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode}");
            }
        }

        // Richiesta dati sulle richieste di adesione per le aziende agricole
        public static async Task<List<AziendaAgricolaModel>> GetRichiesteAziendeAgricoleFromApi(HttpContext context)
        {
            string urlTask = urlGenerico + "aziendaIdrica/richiesteAziendeAgricole/";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["AccessToken"]);

            // Esegue la chiamata POST
            HttpResponseMessage response = await httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<AziendaAgricolaModel>? listaAziende = JsonConvert.DeserializeObject<List<AziendaAgricolaModel>>(responseData);
                if (listaAziende != null)
                    return listaAziende;
                else
                    throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode} - listaAziende = null");
            }
            else
            {
                throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode}");
            }
        }

        // Richiesta dati per limiti di vendita per ogni azienda agricola in base alla azienda idrica
        public static async Task<List<LimiteAcquistoAzienda>> GetLimitiPerAziendaFromApi(HttpContext context)
        {
            string urlTask = urlGenerico + "aziendaIdrica/limitiPerAzienda/";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["AccessToken"]);

            // Esegue la chiamata
            HttpResponseMessage response = await ApiReq.httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<LimiteAcquistoAzienda>? limiti = JsonConvert.DeserializeObject<List<LimiteAcquistoAzienda>>(responseData);
                if (limiti != null)
                    return limiti;
                else
                    throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode} - limiti = null");
            }
            else
            {
                throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode}");
            }
        }


        // Richiesta dati storico vendite azienda idrica
        public static async Task<List<OrdineAcquisto>> GetStoricoVenditeFromApi(string partitaIva, HttpContext context)
        {
            string urlTask = $"{urlGenerico}aziendaIdrica/storicoVendite/?PartitaIva={Uri.EscapeDataString(partitaIva)}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["AccessToken"]);

            // Esegue la chiamata
            HttpResponseMessage response = await ApiReq.httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<OrdineAcquisto>? storicoVendite = JsonConvert.DeserializeObject<List<OrdineAcquisto>>(responseData);
                if (storicoVendite != null)
                    return storicoVendite;
                else
                    throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode} - storicoVendite = null");
            }
            else
            {
                throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode}");
            }
        }


        // Richiesta dati consumo aziende agricole a cui l'azienda idrica ha venduto
        public static async Task<List<ConsumoAziendaleCampo>> GetConsumoAziendeFromApi(string partitaIva, HttpContext context)
        {
            string urlTask = $"{urlGenerico}aziendaIdrica/consumiAziende/?PartitaIva={Uri.EscapeDataString(partitaIva)}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["AccessToken"]);

            // Esegue la chiamata
            HttpResponseMessage response = await ApiReq.httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<ConsumoAziendaleCampo>? listaConsumi = JsonConvert.DeserializeObject<List<ConsumoAziendaleCampo>>(responseData);
                if (listaConsumi != null)
                    return listaConsumi;
                else
                    throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode} - listaConsumi = null");
            }
            else
            {
                throw new HttpRequestException($"Errore nella chiamata API: {response.StatusCode}");
            }
        }




    }
}
