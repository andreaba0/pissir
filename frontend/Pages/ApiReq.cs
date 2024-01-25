using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ServiceStack.Logging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace frontend.Pages
{
    public static class ApiReq
    {
        // Variabili comuni alle classi
        public static readonly string urlGenerico = "http://localhost";
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
                Email = "mariorossi@mail.mail",
                Role = "FAR" //WSP / FAR
            };
            
        }

        // Metodi richieste API

        // Richiesta dati utente
        public static async Task<Utente> GetUserDataFromApi(HttpContext context)
        {
            // Controllo utente autenticato
            if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");

            // Stringa interpolata
            string urlTask = $"{urlGenerico}/profile";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

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
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }

        // Richiesta dati adesione utente
        public static async Task<UtenteAp> GetUserDataApplicationFromApi(HttpContext context)
        {
            // Controllo utente autenticato
            if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");

            // Controllo utente autorizzato
            if (utente.Role != "WSP") { throw new Exception("Unauthorized"); }

            // Stringa interpolata
            string urlTask = $"{urlGenerico}/service/my_application";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

            // Esegue la chiamata
            HttpResponseMessage response = await httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                UtenteAp? userData = JsonConvert.DeserializeObject<UtenteAp>(responseData);

                if (userData != null)
                    return userData;
                else
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }

        // Richiesta dati azienda generica
        public static async Task<Azienda> GetAziendaDataFromApi(HttpContext context)
        {
            // Controllo utente autenticato
            if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");

            string urlTask = $"{urlGenerico}/company";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

            // Esegue la chiamata
            HttpResponseMessage response = await ApiReq.httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                Azienda? aziendaData = JsonConvert.DeserializeObject<Azienda>(responseData);
                if (aziendaData != null)
                    return aziendaData;
                else
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }

        // Richiesta dati azienda idrica
        public static async Task<AziendaIdricaModel> GetAziendaIdricaDataFromApi(HttpContext context)
        {
            // Controllo utente autenticato
            if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");

            string urlTask = $"{urlGenerico}/watercompany";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

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
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }


        // Richiesta dati azienda agricola
        public static async Task<AziendaAgricolaModel> GetAziendaAgricolaDataFromApi(HttpContext context)
        {
            // Controllo utente autenticato
            if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");

            string urlTask = $"{urlGenerico}/farmcompany";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

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
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }




        // Metodi Azienda Agricola -----------------------------------------------

        // Richiesta dati sulle colture possedute dall'azienda
        public static async Task<List<Coltura>> GetColtureAziendaFromApi(HttpContext context)
        {
            // Controllo utente autenticato
            if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");

            // Controllo utente autorizzato
            if (utente.Role != "FAR") { throw new Exception("Unauthorized"); }

            string urlTask = $"{urlGenerico}/field";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

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
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }

        // Richiesta dati sulle quantità d'acqua rimanenti per i campi posseduti dall'azienda
        public static async Task<List<ColturaStock>> GetStockColtureAziendaFromApi(HttpContext context)
        {
            // Controllo utente autenticato
            if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");

            // Controllo utente autorizzato
            if (utente.Role != "FAR") { throw new Exception("Unauthorized"); }

            string urlTask = $"{urlGenerico}/water/stock";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

            // Esegue la chiamata
            HttpResponseMessage response = await httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<ColturaStock>? lista = JsonConvert.DeserializeObject<List<ColturaStock>>(responseData);
                if (lista != null)
                    return lista;
                else
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }

        // Richiesta dati per limite giornaliero di acquisto della singola azienda
        public static async Task<float> GetLimiteAcquistoAziendaFromApi(HttpContext context)
        {
            // Controllo utente autenticato
            if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");

            string urlTask = $"{urlGenerico}/water/limit";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

            // Esegue la chiamata POST
            HttpResponseMessage response = await ApiReq.httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                float limite = JsonConvert.DeserializeObject<float>(responseData);
                if (responseData != null)
                    return limite;
                else
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }

        // Richiesta dati per la stima di risorse idriche per quel campo
        public static async Task<List<AcquaStimata>> GetStimeCampiPerAziendaFromApi(HttpContext context)
        {
            // Controllo utente autenticato
            if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");

            string urlTask = $"{urlGenerico}/water/reccomendation";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

            // Esegue la chiamata POST
            HttpResponseMessage response = await ApiReq.httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<AcquaStimata>? rec = JsonConvert.DeserializeObject<List<AcquaStimata>>(responseData);
                if (rec != null)
                    return rec;
                else
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }

        // Richiesta dati sulla lista di offerte delle aziende idriche
        public static async Task<List<Offerta>> GetOfferteIdricheFromApi(HttpContext context)
        {
            // Controllo utente autenticato
            if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");

            string urlTask = $"{urlGenerico}/water/offer";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

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
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }

        //TODO
        // Richiesta dati dello storico ordini d'acqua
        public static async Task<List<OrdineAcquisto>> GetStoricoOrdiniFromApi(string partitaIva, HttpContext context)
        {
            // Controllo utente autenticato
            if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");

            // Controllo utente autorizzato
            if (utente.Role != "FAR") { throw new Exception("Unauthorized"); }

            string urlTask = $"{urlGenerico}aziendaAgricola/ordini/?PartitaIva={Uri.EscapeDataString(partitaIva)}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

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
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }


        //TODO
        // Richiesta dati sui consumi delle colture possedute dall'azienda
        public static async Task<List<ConsumoAziendaleCampo>> GetStoricoConsumiFromApi(string partitaIva, HttpContext context)
        {
            // Controllo utente autenticato
            if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");

            // Controllo utente autorizzato
            if (utente.Role != "FAR") { throw new Exception("Unauthorized"); }

            string urlTask = $"{urlGenerico}aziendaAgricola/consumiColture/?PartitaIva={Uri.EscapeDataString(partitaIva)}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

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
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }


        //TODO
        // Richiesta dati storico sensori di umidità
        public static async Task<List<SensoreUmiditaLog>> GetSensoriUmiditaFromApi(string partitaIva, HttpContext context)
        {
            // Controllo utente autenticato
            if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");

            string urlTask = $"{urlGenerico}aziendaAgricola/storicoSensoriUmidita/?PartitaIva={Uri.EscapeDataString(partitaIva)}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

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
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }

        //TODO
        // Richiesta dati storico sensori di temperatura
        public static async Task<List<SensoreTemperaturaLog>> GetSensoriTemperaturaFromApi(string partitaIva, HttpContext context)
        {
            // Controllo utente autenticato
            if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");

            string urlTask = $"{urlGenerico}aziendaAgricola/storicoSensoriTemperatura/?PartitaIva={Uri.EscapeDataString(partitaIva)}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

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
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }

        //TODO
        // Richiesta dati storico attuatori
        public static async Task<List<AttuatoreLog>> GetAttuatoriFromApi(string partitaIva, HttpContext context)
        {
            // Controllo utente autenticato
            if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");

            string urlTask = $"{urlGenerico}aziendaAgricola/storicoAttuatori/?PartitaIva={Uri.EscapeDataString(partitaIva)}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

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
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }







        // Metodi Azienda Idrica ---------------------------------------------------------

        /*
        // Richiesta dati per limite giornaliero di vendita
        public static async Task<float> GetLimiteGiornaliero(string partitaIva, HttpContext context)
        {
            // Controllo utente autenticato
            if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");

            string urlTask = $"{urlGenerico}aziendaIdrica/limiteGiornaliero/?PartitaIva={Uri.EscapeDataString(partitaIva)}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

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
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }
        */
        
        // Richiesta dati sulle richieste di adesione per gli utenti
        public static async Task<List<UtenteAp>> GetRichiesteUtentiFromApi(HttpContext context)
        {
            // Controllo utente autenticato
            if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");

            // Controllo utente autorizzato
            if (utente.Role != "WSP") { throw new Exception("Unauthorized"); }

            string urlTask = urlGenerico + "/service/application";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

            // Esegue la chiamata
            HttpResponseMessage response = await httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<UtenteAp>? listaUtenti = JsonConvert.DeserializeObject<List<UtenteAp>>(responseData);
                if (listaUtenti != null)
                    return listaUtenti;
                else
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }
        

        // Richiesta dati sulle richieste di adesione per gli utenti
        public static async Task<List<UtentePeriodo>> GetRichiestePeriodoFromApi(HttpContext context)
        {
            // Controllo utente autenticato
            if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");

            // Controllo utente autorizzato
            if (utente.Role != "WSP") { throw new Exception("Unauthorized"); }

            string urlTask = urlGenerico + "/apiaccess";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

            // Esegue la chiamata POST
            HttpResponseMessage response = await httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                List<UtentePeriodo>? listaUtenti = JsonConvert.DeserializeObject<List<UtentePeriodo>>(responseData);
                if (listaUtenti != null)
                    return listaUtenti;
                else
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }

        //TODO
        // Richiesta dati per limiti di vendita per ogni azienda agricola in base alla azienda idrica
        public static async Task<List<LimiteAcquistoAzienda>> GetLimitiPerAziendaFromApi(HttpContext context)
        {
            // Controllo utente autenticato
            if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");

            string urlTask = urlGenerico + "/water/limitall";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

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
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }

        //TODO
        // Richiesta dati storico vendite azienda idrica
        public static async Task<List<OrdineAcquisto>> GetStoricoVenditeFromApi(HttpContext context)
        {
            // Controllo utente autenticato
            if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");

            // Controllo utente autorizzato
            if (utente.Role != "WSP") { throw new Exception("Unauthorized"); }

            string urlTask = $"{urlGenerico}/aziendaIdrica/storicoVendite";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

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
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }

        //TODO
        // Richiesta dati consumo aziende agricole a cui l'azienda idrica ha venduto
        public static async Task<List<ConsumoAziendaleCampo>> GetConsumoAziendeFromApi(HttpContext context)
        {
            // Controllo utente autenticato
            if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");

            // Controllo utente autorizzato
            if (utente.Role != "WSP") { throw new Exception("Unauthorized"); }

            string urlTask = $"{urlGenerico}/aziendaIdrica/consumiAziende";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

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
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }



        // Controllo utente autenticato
        public static async Task<bool> IsUserAuth(HttpContext context)
        {
            // Verifica l'esistenza del cookie
            if (!context.Request.Cookies.TryGetValue("Token", out string token))
            {
                return false;
            }

            string urlTask = $"{urlGenerico}/profile";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Esegue la chiamata
            HttpResponseMessage response = await httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge e deserializza i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                Utente? userData = JsonConvert.DeserializeObject<Utente>(responseData);
                if (userData != null)
                {
                    utente = userData;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
