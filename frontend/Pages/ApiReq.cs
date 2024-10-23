using frontend.Models;
using Newtonsoft.Json;
using ServiceStack;
using System.Net.Http.Headers;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;

namespace frontend.Pages
{
    public static class ApiReq
    {
        // Variabili comuni alle classi
        //public static readonly string urlGenerico = Environment.GetEnvironmentVariable("ipbackend") ?? throw new InvalidOperationException("La variabile d'ambiente 'ipbackend' non è impostata.");
        public static readonly string authUrlGenerico = Environment.GetEnvironmentVariable("ipbackend_auth") ?? throw new InvalidOperationException("La variabile d'ambiente 'ipbackend_auth' non è impostata.");
        public static readonly string apiUrlGenerico = Environment.GetEnvironmentVariable("ipbackend_api") ?? throw new InvalidOperationException("La variabile d'ambiente 'ipbackend_api' non è impostata.");
        public static readonly HttpClient httpClient = new();
        public static Utente? utente { get; set; }

        // Metodo richieste API
        public static async Task<string> GetDataFromApi(HttpContext context, string endpoint, bool checkAuth = true, bool useApiToken = false)
        {
            // Controllo utente autenticato
            if (checkAuth)
            {
                if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");
            }

            string cookieNameToUse = "";
            if (useApiToken)
            {
                cookieNameToUse = "ApiToken";
                await GetApiToken(context);
            }
            else
            {
                cookieNameToUse = "Token";
            }

            // Stringa interpolata
            string urlTask = $"{apiUrlGenerico}{endpoint}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies[cookieNameToUse]);

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

        public static async Task<string> GetDataFromAuth(HttpContext context, string endpoint, bool checkAuth = true)
        {
            // Controllo utente autenticato
            if (checkAuth)
            {
                if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");
            }

            string cookieNameToUse = "Token";

            // Stringa interpolata
            string urlTask = $"{authUrlGenerico}{endpoint}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies[cookieNameToUse]);

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

        // Metodo per ottenere l'API Token per l'accesso alle api
        public static async Task GetApiToken(HttpContext context, bool checkAuth = true)
        {
            // Controllo api token esistente
            if (!context.Request.Cookies["ApiToken"].IsNullOrEmpty()) { return; }

            // Controllo utente autenticato
            if (checkAuth)
            {
                if (!await IsUserAuth(context)) context.Response.Redirect("/auth/SignIn");
            }

            // Stringa interpolata
            string urlTask = $"{authUrlGenerico}/token";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["Token"]);

            HttpResponseMessage response = await httpClient.GetAsync(urlTask);

            if (response.IsSuccessStatusCode)
            {
                // Legge i dati dalla risposta
                string responseData = await response.Content.ReadAsStringAsync();
                //Console.WriteLine("---------------------- API TOKEN: " + responseData);

                if (responseData != null)
                {
                    // Decodifica jwt per ottenere exp date
                    var jwtHandler = new JwtSecurityTokenHandler();
                    var jwtToken = jwtHandler.ReadJwtToken(responseData);
                    var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;

                    if (expClaim != null)
                    {
                        // Converte il valore 'exp' (timestamp Unix) in DateTime
                        var expirationTimeUnix = long.Parse(expClaim);
                        var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expirationTimeUnix).DateTime;

                        // Salvataggio dell'apitoken nei cookie
                        context.Response.Cookies.Append("ApiToken", responseData, new CookieOptions
                        {
                            Path = "/",
                            Expires = expirationTime,
                            HttpOnly = true,
                            Secure = false,
                        });
                    }
                    else
                    {
                        // Salvataggio dell'apitoken nei cookie
                        context.Response.Cookies.Append("ApiToken", responseData, new CookieOptions
                        {
                            Path = "/",
                            Expires = DateTime.Now.AddHours(1),
                            HttpOnly = true,
                            Secure = false,
                        });
                    }
                }
                else
                    throw new HttpRequestException($"{response.StatusCode}");
            }
            else
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"{response.StatusCode}");
            }
            return;
        }
       
        // Controllo utente autenticato
        public static async Task<bool> IsUserAuth(HttpContext context)
        {
            // Verifica l'esistenza del cookie
            if (!context.Request.Cookies.TryGetValue("Token", out string token)) { return false; }

            string urlTask = $"{authUrlGenerico}/profile";

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
