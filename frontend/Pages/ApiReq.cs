using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace frontend.Pages
{
    public static class ApiReq
    {
        // Variabili comuni alle classi
        public static string urlGenerico = "http://tuo-api-url.com/api/";
        public static HttpClient httpClient = new();
        public static Utente? utente { get; set; }

        //Costruttore statico per testare
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
                Role = "GSI" //GSI / GAA
            };
        }

        // Metodi richieste API

        // Richiesta dati utente
        public static async Task<Utente> GetUserDataFromApi(string codFiscale)
        {
            string urlTask = urlGenerico + "user/";

            // Puoi impostare eventuali intestazioni necessarie qui
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "IlTuoToken");

            // Creare il corpo della richiesta
            var requestBody = new { CodiceFiscale = codFiscale };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata POST
            HttpResponseMessage response = await httpClient.PostAsync(urlTask, content);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                Utente userData = JsonConvert.DeserializeObject<Utente>(responseData);
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
        public static async Task<AziendaIdricaModel> GetAziendaIdricaDataFromApi(string partitaIva)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaIdrica/";

            // Puoi impostare eventuali intestazioni necessarie qui
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "IlTuoToken");

            // Creare il corpo della richiesta
            var requestBody = new { PartitaIva = partitaIva };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata POST
            HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                AziendaIdricaModel aziendaData = JsonConvert.DeserializeObject<AziendaIdricaModel>(responseData);
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
        public static async Task<AziendaAgricolaModel> GetAziendaAgricolaDataFromApi(string partitaIva)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaAgricola/";

            // Puoi impostare eventuali intestazioni necessarie qui
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "IlTuoToken");

            // Creare il corpo della richiesta
            var requestBody = new { PartitaIva = partitaIva };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata POST
            HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                AziendaAgricolaModel aziendaData = JsonConvert.DeserializeObject<AziendaAgricolaModel>(responseData);
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
        public static async Task<List<Coltura>> GetColtureAziendaFromApi(string partitaIva)
        {
            string urlTask = urlGenerico + "aziendaAgricola/colture";

            // Puoi impostare eventuali intestazioni necessarie qui
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "IlTuoToken");

            // Creare il corpo della richiesta
            var requestBody = new { PartitaIva = partitaIva };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata POST
            HttpResponseMessage response = await httpClient.PostAsync(urlTask, content);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<Coltura> listaColture = JsonConvert.DeserializeObject<List<Coltura>>(responseData);
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
        public static async Task<List<Offerta>> GetOfferteIdricheFromApi()
        {
            string urlTask = ApiReq.urlGenerico + "aziendaAgricola/offerteIdriche";

            // Puoi impostare eventuali intestazioni necessarie qui
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "IlTuoToken");


            // Esegue la chiamata POST
            HttpResponseMessage response = await ApiReq.httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<Offerta> listaOfferte = JsonConvert.DeserializeObject<List<Offerta>>(responseData);
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


        // Richiesta dati sui consumi delle colture possedute dall'azienda
        public static async Task<List<ConsumoAziendaleCampo>> GetStoricoConsumiFromApi(string partitaIva)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaAgricola/consumiColture";

            // Puoi impostare eventuali intestazioni necessarie qui
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "IlTuoToken");

            // Creare il corpo della richiesta
            var requestBody = new { PartitaIva = partitaIva };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata POST
            HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<ConsumoAziendaleCampo> listaConsumi = JsonConvert.DeserializeObject<List<ConsumoAziendaleCampo>>(responseData);
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
        public static async Task<List<SensoreUmiditaLog>> GetSensoriUmiditaFromApi(string partitaIva)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaAgricola/storicoSensoriUmidita";

            // Puoi impostare eventuali intestazioni necessarie qui
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "IlTuoToken");

            // Creare il corpo della richiesta
            var requestBody = new { PartitaIva = partitaIva };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata POST
            HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<SensoreUmiditaLog> lista = JsonConvert.DeserializeObject<List<SensoreUmiditaLog>>(responseData);
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
        public static async Task<List<SensoreTemperaturaLog>> GetSensoriTemperaturaFromApi(string partitaIva)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaAgricola/storicoSensoriTemperatura";

            // Puoi impostare eventuali intestazioni necessarie qui
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "IlTuoToken");

            // Creare il corpo della richiesta
            var requestBody = new { PartitaIva = partitaIva };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata POST
            HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<SensoreTemperaturaLog> lista = JsonConvert.DeserializeObject<List<SensoreTemperaturaLog>>(responseData);
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
        public static async Task<List<AttuatoreLog>> GetAttuatoriFromApi(string partitaIva)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaAgricola/storicoAttuatori";

            // Puoi impostare eventuali intestazioni necessarie qui
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "IlTuoToken");

            // Creare il corpo della richiesta
            var requestBody = new { PartitaIva = partitaIva };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata POST
            HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<AttuatoreLog> lista = JsonConvert.DeserializeObject<List<AttuatoreLog>>(responseData);
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
        public static async Task<float> GetLimiteGiornaliero(string partitaIva)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaIdrica/limiteGiornaliero/";

            // Puoi impostare eventuali intestazioni necessarie qui
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "IlTuoToken");

            // Creare il corpo della richiesta
            var requestBody = new { PartitaIva = partitaIva };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata POST
            HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                float limite = 0;
                limite = JsonConvert.DeserializeObject<float>(responseData);
                if (limite == 0)
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
        public static async Task<float> GetAcquaDisponibile(string partitaIva)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaIdrica/limiteGiornaliero/";

            // Puoi impostare eventuali intestazioni necessarie qui
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "IlTuoToken");

            // Creare il corpo della richiesta
            var requestBody = new { PartitaIva = partitaIva };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata POST
            HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                float acqua = 0;
                acqua = JsonConvert.DeserializeObject<float>(responseData);
                if (acqua == 0)
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
        public static async Task<List<Utente>> GetRichiesteUtentiFromApi()
        {
            string urlTask = urlGenerico + "aziendaIdrica/richiesteUtenti/";

            // Puoi impostare eventuali intestazioni necessarie qui
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "IlTuoToken");

            // Esegue la chiamata POST
            HttpResponseMessage response = await httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<Utente> listaUtenti = JsonConvert.DeserializeObject<List<Utente>>(responseData);
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
        public static async Task<List<AziendaAgricolaModel>> GetRichiesteAziendeAgricoleFromApi()
        {
            string urlTask = urlGenerico + "aziendaIdrica/richiesteAziendeAgricole/";

            // Puoi impostare eventuali intestazioni necessarie qui
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "IlTuoToken");

            // Esegue la chiamata POST
            HttpResponseMessage response = await httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<AziendaAgricolaModel> listaAziende = JsonConvert.DeserializeObject<List<AziendaAgricolaModel>>(responseData);
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
        public static async Task<List<LimiteAcquistoAzienda>> GetLimitiPerAziendaFromApi(string partitaIva)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaIdrica/limitiPerAzienda/";

            // Puoi impostare eventuali intestazioni necessarie qui
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "IlTuoToken");

            // Creare il corpo della richiesta
            var requestBody = new { PartitaIva = partitaIva };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata POST
            HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<LimiteAcquistoAzienda> limiti = JsonConvert.DeserializeObject<List<LimiteAcquistoAzienda>>(responseData);
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
        public static async Task<List<OrdineAcquisto>> GetStoricoVenditeFromApi(string partitaIva)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaIdrica/storicoVendite";

            // Puoi impostare eventuali intestazioni necessarie qui
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "IlTuoToken");

            // Creare il corpo della richiesta
            var requestBody = new { PartitaIva = partitaIva };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata POST
            HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<OrdineAcquisto> storicoVendite = JsonConvert.DeserializeObject<List<OrdineAcquisto>>(responseData);
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
        public static async Task<List<ConsumoAziendaleCampo>> GetConsumoAziendeFromApi(string partitaIva)
        {
            string urlTask = ApiReq.urlGenerico + "aziendaIdrica/consumiAziende";

            // Puoi impostare eventuali intestazioni necessarie qui
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "IlTuoToken");

            // Creare il corpo della richiesta
            var requestBody = new { PartitaIva = partitaIva };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Esegue la chiamata POST
            HttpResponseMessage response = await ApiReq.httpClient.PostAsync(urlTask, content);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<ConsumoAziendaleCampo> listaConsumi = JsonConvert.DeserializeObject<List<ConsumoAziendaleCampo>>(responseData);
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
