using frontend.Pages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

public class SignInModel : PageModel
{
    string authGoogle = "https://accounts.google.com/o/oauth2/auth?" +
        "scope=openid&" +
        "access_type=online&" +
        "response_type=code&" +
        "state=1234567890qwerty&" +
        "redirect_uri=https%3A//redirect.localhost.andreabarchietto.it/oauth/google&" +
        "client_id=218970200448-62ddcs49ilsub6a1r1l2k2vpd84elkqs.apps.googleusercontent.com";

    string authFacebook = "https://www.facebook.com/v18.0/dialog/oauth?" +
        "scope=openid&" +
        "response_type=code&" +
        "state=1234567890qwerty&" +
        "redirect_uri=https%3A//redirect.localhost.andreabarchietto.it/oauth/facebook&" +
        "client_id=709636884633879&" +
        "client_secret=f502cde6c8b5dec3d1046ceef2aa78e8";

    public async Task<IActionResult> OnGet()
    {
        string? provider = Request.Query["provider"];
        string? code = Request.Query["code"];

        // Se esiste il provider
        if (!string.IsNullOrEmpty(provider))
        {
            Response.Cookies.Append("Provider", provider, new CookieOptions
            {
                Path = "/",
                Expires = DateTime.Now.AddMinutes(1),
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
                clientId = "218970200448-62ddcs49ilsub6a1r1l2k2vpd84elkqs.apps.googleusercontent.com";
                clientSecret = "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";
                redirectUri = "https://redirect.localhost.andreabarchietto.it/oauth/google";
                break;

            case "facebook":
                tokenEndpoint = "https://graph.facebook.com/v18.0/oauth/access_token";
                clientId = "709636884633879";
                clientSecret = "f502cde6c8b5dec3d1046ceef2aa78e8";
                redirectUri = "https://redirect.localhost.andreabarchietto.it/oauth/facebook";
                break;

            default:
                // Provider non supportato
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
                        Response.Cookies.Append("AccessToken", accessToken, new CookieOptions
                        {
                            Path = "/",
                            Expires = expirationTime,
                            HttpOnly = true,
                            Secure = false
                        });


                    }
                    else
                    {
                        // Gestisci il caso in cui "expires_in" non è presente o non è un numero valido
                        TempData["MessaggioErrore"] = "Expires_in non presente o non valido. Autenticazione fallita.";
                        return RedirectToPage();
                    }



                    if (accessToken != null)
                    {
                        return RedirectToPage("/DatiAccount");
                        ApiReq.utente = await ApiReq.GetUserDataFromApi(HttpContext);

                        // Se è stato accettato nel sistema
                        if (!string.IsNullOrEmpty(ApiReq.utente.Role) && !string.IsNullOrEmpty(ApiReq.utente.PartitaIva))
                            return RedirectToPage("/DatiAccount");
                        else
                            return RedirectToPage("/auth/SignToFarm");
                    }



                }
                else
                {
                    // Gestisci il caso in cui il token non è presente nella risposta
                    TempData["MessaggioErrore"] = "Access Token non presente o non valido. Autenticazione fallita.";
                    return RedirectToPage();
                }
            }
        }


        return RedirectToPage();
    }

    


}


