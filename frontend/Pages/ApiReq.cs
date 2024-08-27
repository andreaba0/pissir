using frontend.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace frontend.Pages
{
    public static class ApiReq
    {
        // Variabili comuni alle classi
        public static readonly string urlGenerico = Environment.GetEnvironmentVariable("ipbackend") ?? throw new InvalidOperationException("La variabile d'ambiente 'ipbackend' non è impostata.");
        public static readonly HttpClient httpClient = new();
        public static Utente? utente { get; set; }

        // Metodo richieste API
        public static async Task<string> GetDataFromApi(HttpContext context, string endpoint, bool checkAuth = true)
        {
            // Controllo utente autenticato
            if (checkAuth)
            {
                if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");
            }
            

            // Stringa interpolata
            string urlTask = $"{urlGenerico}{endpoint}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

            HttpResponseMessage response = await httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                if (responseData != null)
                    return responseData;
                else
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"{response.StatusCode}");
            }
        }


       
        // Controllo utente autenticato
        public static async Task<bool> IsUserAuth(HttpContext context)
        {
            // Verifica l'esistenza del cookie
            if (!context.Request.Cookies.TryGetValue("Token", out string token)) { return false; }

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
