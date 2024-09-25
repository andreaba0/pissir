using frontend.Models;
using frontend.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net;

public class SignInModel : PageModel
{
    public async Task<IActionResult> OnGet()
    {
        try
        {
            // Controllo variabili d'ambiente
            string[] requiredVariables = { "ipbackend", "googleClientId", "googleSecretId", "facebookClientId", "facebookSecretId" };

            foreach (string variable in requiredVariables)
            {
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(variable)))
                {
                    TempData["MessaggioErrore"] = $"La variabile d'ambiente '{variable}' non è definita.";
                    return Redirect("/Error");
                }
            }

            string authGoogle = "https://accounts.google.com/o/oauth2/auth?" +
                "scope=openid&" +
                "access_type=online&" +
                "response_type=code&" +
                "state=1234567890qwerty&" +
                "redirect_uri=https%3A//appweb.andreabarchietto.it/localhost_redirect/oauth/google&" +
                "client_id=" + Environment.GetEnvironmentVariable("googleClientId");

            string authFacebook = "https://www.facebook.com/v18.0/dialog/oauth?" +
                "scope=openid&" +
                "response_type=code&" +
                "state=1234567890qwerty&" +
                "redirect_uri=https%3A//appweb.andreabarchietto.it/localhost_redirect/oauth/facebook&" +
                "client_id=" + Environment.GetEnvironmentVariable("facebookClientId");

            // Richiesta parametri nell'url
            string? provider = Request.Query["provider"];
            string? code = Request.Query["code"];
            provider = provider?.ToLower();

            HttpContext.Response.Cookies.Delete("Code");

            // Se esiste già il token
            string tk = HttpContext.Request.Cookies["Token"];
            if (!string.IsNullOrEmpty(tk))
            {
                try
                {
                    string data = await ApiReq.GetDataFromApi(HttpContext, "/profile");
                    ApiReq.utente = JsonConvert.DeserializeObject<Utente>(data);
                    return RedirectToPage("/DatiAccount");
                }
                catch (HttpRequestException ex)
                {
                    string statusCode = ex.Message.ToString().ToLower();

                    if (statusCode == "unauthorized")
                    {
                        TempData["MessaggioErrore"] = "Errore 401. Non sei autorizzato.";
                    }
                    else if (statusCode == "not found" || statusCode == "notfound")
                    {
                        //TempData["MessaggioErrore"] = "Errore 404";
                        return RedirectToPage("/auth/SignToFarm");
                    }
                    else
                    {
                        TempData["MessaggioErrore"] = $"Errore: {ex.Message}. Riprovare più tardi.";
                    }

                    return RedirectToPage("/Error");
                }
            }

            // Redirect al giusto provider per ottenere il token, se esiste il provider e il relativo code
            if (!string.IsNullOrEmpty(provider) && string.IsNullOrEmpty(code))
            {
                if (provider == "google")
                {
                    return Redirect(authGoogle);
                }
                else if (provider == "facebook")
                {
                    return Redirect(authFacebook);
                }
                else
                {
                    TempData["MessaggioErrore"] = "Errore nella selezione del provider.";
                    return RedirectToPage();
                }
            }


            if (!string.IsNullOrEmpty(code))
            {
                // Salvataggio del code nei cookie
                Response.Cookies.Append("Code", code, new CookieOptions
                {
                    Path = "/",
                    Expires = DateTime.Now.AddMinutes(1),
                    HttpOnly = true,
                    Secure = false,
                });

            }

            if (string.IsNullOrEmpty(provider))
            {
                return Page();
            }

            // Prende il tipo di provider per reinderizzare all'url giusto per la creazione del token
            string tokenEndpoint = "";
            string clientId = "";
            string clientSecret = "";
            string redirectUri = "";

            switch (provider)
            {
                case "google":
                    tokenEndpoint = "https://oauth2.googleapis.com/token";
                    clientId = Environment.GetEnvironmentVariable("googleClientId");
                    clientSecret = Environment.GetEnvironmentVariable("googleSecretId");
                    redirectUri = "https://appweb.andreabarchietto.it/localhost_redirect/oauth/google";
                    break;

                case "facebook":
                    tokenEndpoint = "https://graph.facebook.com/v18.0/oauth/access_token";
                    clientId = Environment.GetEnvironmentVariable("facebookClientId");
                    clientSecret = Environment.GetEnvironmentVariable("facebookSecretId");
                    redirectUri = "https://appweb.andreabarchietto.it/localhost_redirect/oauth/facebook";
                    break;

                default:
                    TempData["MessaggioErrore"] = "Provider non supportato.";
                    return RedirectToPage();
            }

            if (provider != null && code != null)
            {
                HttpContext.Response.Cookies.Delete("Code");

                var tokenRequestData = new List<KeyValuePair<string, string>>
                {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret)
                };

                var tokenRequest = new FormUrlEncodedContent(tokenRequestData);

                using (var httpClient = new HttpClient())
                {
                    var tokenResponse = await httpClient.PostAsync(tokenEndpoint, tokenRequest);
                    var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();

                    // Estrai il token di accesso dalla risposta JSON
                    var tokenResponseObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(tokenResponseContent);

                    if (tokenResponseObject != null && tokenResponseObject.ContainsKey("id_token"))
                    {
                        var accessToken = tokenResponseObject["id_token"];

                        // Estrai l'expiration time dal token di accesso
                        if (tokenResponseObject.ContainsKey("expires_in") && int.TryParse(tokenResponseObject["expires_in"], out var expiresIn))
                        {
                            // Calcola il tempo di scadenza del cookie
                            var expirationTime = DateTime.Now.AddSeconds(expiresIn);

                            // Memorizza il token di accesso nel cookie con l'expiration time
                            Response.Cookies.Append("Token", accessToken, new CookieOptions
                            {
                                Path = "/",
                                Expires = expirationTime,
                                HttpOnly = true,
                                Secure = false
                            });
                        }
                        else
                        {
                            // Gestisci il caso in cui "expires_in" non presente o non un numero valido
                            TempData["MessaggioErrore"] = "Expires_in non presente o non valido. Autenticazione fallita.";
                            return RedirectToPage();
                        }


                        // Gestione redirect alle pagine dopo il login
                        if (accessToken != null)
                        {
                            await RouteToPage();
                        }
                    }
                    else
                    {
                        // Gestisci il caso in cui il token non presente nella risposta
                        TempData["MessaggioErrore"] = "Access Token non presente o non valido. Autenticazione fallita.";
                        return RedirectToPage();
                    }
                }
            }

            return RedirectToPage();

        }
        catch (Exception ex)
        {
            ViewData["MessaggioErrore"] = "Si è verificato un errore durante l'accesso. " + ex.ToString();
            return RedirectToPage("/Error");
        }

    }


    private async Task<RedirectToPageResult> RouteToPage()
    {
        try
        {
            string data = await ApiReq.GetDataFromApi(HttpContext, "/profile");
            ApiReq.utente = JsonConvert.DeserializeObject<Utente>(data);
            return RedirectToPage("/DatiAccount");
        }
        catch (HttpRequestException ex)
        {
            string statusCode = ex.Message.ToString().ToLower();

            if (statusCode == "unauthorized")
            {
                //TempData["MessaggioErrore"] = "Errore 401. Non sei autorizzato.";
                return RedirectToPage("/Error");
            }
            else if (statusCode == "not found" || statusCode == "notfound")
            {
                //TempData["MessaggioErrore"] = "Errore 404";
                return RedirectToPage("/auth/SignToFarm");
            }
            else
            {
                TempData["MessaggioErrore"] = $"Errore: {ex.Message}. Riprovare più tardi.";
            }

            return RedirectToPage("/Error");
        }
    }
}

