using frontend.Pages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using ServiceStack.MiniProfiler;
using System.Net;

public class SignInModel : PageModel
{
    string authGoogle = "https://accounts.google.com/o/oauth2/auth?" +
        "scope=openid&" +
        "access_type=online&" +
        "response_type=code&" +
        "state=1234567890qwerty&" +
        "redirect_uri=https%3A//appweb.andreabarchietto.it/localhost_redirect/oauth/google&" +
        "client_id=330493585576-us7lib6fpk4bg0j1vcti09l0jpso2o4k.apps.googleusercontent.com";

    string authFacebook = "https://www.facebook.com/v18.0/dialog/oauth?" +
        "scope=openid&" +
        "response_type=code&" +
        "state=1234567890qwerty&" +
        "redirect_uri=https%3A//appweb.andreabarchietto.it/localhost_redirect/oauth/facebook&" +
        "client_id=709636884633879&" +
        "client_secret=f502cde6c8b5dec3d1046ceef2aa78e8";

    public async Task<IActionResult> OnGet()
    {
        try
        {
            Console.WriteLine("Provider: " + Request.Query["provider"]);
            string? provider = Request.Query["provider"];
            string? code = Request.Query["code"];

            //HttpContext.Response.Cookies.Delete("Provider");
            HttpContext.Response.Cookies.Delete("Code");

            // Se esiste il provider
            if (!string.IsNullOrEmpty(provider))
            {
                Response.Cookies.Append("Provider", provider, new CookieOptions
                {
                    Path = "/",
                    Expires = DateTime.Now.AddSeconds(10),
                    HttpOnly = true,
                    Secure = false,
                });
            }

            if (!string.IsNullOrEmpty(provider) && string.IsNullOrEmpty(code))
            {
                provider = provider.ToLower();

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

            // Prende il tipo di provider dal cookie per reinderizzare all'url giusto per la creazione del token
            provider = HttpContext.Request.Cookies["Provider"];

            if (string.IsNullOrEmpty(provider))
            {
                return Page();
            }


            string tokenEndpoint = "";
            string clientId = "";
            string clientSecret = "";
            string redirectUri = "";

            switch (provider.ToLower())
            {
                case "google":
                    tokenEndpoint = "https://oauth2.googleapis.com/token";
                    clientId = "330493585576-us7lib6fpk4bg0j1vcti09l0jpso2o4k.apps.googleusercontent.com";
                    clientSecret = "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";
                    redirectUri = "https://appweb.andreabarchietto.it/localhost_redirect/oauth/google";
                    break;

                case "facebook":
                    tokenEndpoint = "https://graph.facebook.com/v18.0/oauth/access_token";
                    clientId = "709636884633879";
                    clientSecret = "f502cde6c8b5dec3d1046ceef2aa78e8";
                    redirectUri = "https://appweb.andreabarchietto.it/localhost_redirect/oauth/facebook";
                    break;

                default:
                    TempData["MessaggioErrore"] = "Provider non supportato.";
                    return RedirectToPage();
            }


            if (HttpContext.Request.Cookies["Provider"] != null && HttpContext.Request.Cookies["Code"] != null)
            {
                code = HttpContext.Request.Cookies["Code"];

                HttpContext.Response.Cookies.Delete("Provider");
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
                            // Gestisci il caso in cui "expires_in" non � presente o non � un numero valido
                            TempData["MessaggioErrore"] = "Expires_in non presente o non valido. Autenticazione fallita.";
                            return RedirectToPage();
                        }


                        // Gestione redirect alle pagine dopo il login
                        if (accessToken != null)
                        {
                            try
                            {
                                ApiReq.utente = await ApiReq.GetUserDataFromApi(HttpContext);
                                
                                return RedirectToPage("/DatiAccount");
                            }
                            catch (HttpRequestException ex)
                            {
                                string statusCode = ex.Message.ToString().ToLower();

                                if (statusCode == "unauthorized")
                                {
                                    //TempData["MessaggioErrore"] = "Errore 400";
                                    return RedirectToPage("/auth/AuthPeriod");

                                }
                                else if (statusCode == "notfound")
                                {
                                    //TempData["MessaggioErrore"] = "Errore 404";
                                    return RedirectToPage("/auth/SignToFarm");
                                }
                                else
                                {
                                    TempData["MessaggioErrore"] = $"Errore: {ex.Message}. Riprovare pi� tardi.";
                                }
                                
                                return RedirectToPage("/Error");
                            }

                        }



                    }
                    else
                    {
                        // Gestisci il caso in cui il token non � presente nella risposta
                        TempData["MessaggioErrore"] = "Access Token non presente o non valido. Autenticazione fallita.";
                        return RedirectToPage();
                    }
                }
            }


            return RedirectToPage();

        }
        catch (Exception ex)
        {
            ViewData["MessaggioErrore"] = "Si � verificato un errore durante l'accesso. " + ex.ToString();
            return RedirectToPage("/Error");
        }
        
    }

    


}


