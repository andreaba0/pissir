using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

public class SignInModel : PageModel
{
    private readonly IConfiguration _configuration;

    public string? ClientId { get; set; }

    public void OnGet()
    {
        //ClientId = _configuration["oauth:google:client_id"];
    }

    public IActionResult OnGetCallback()
    {
        var token = Request.Query["token"];

        // Salvataggio del token come cookie
        HttpContext.Response.Cookies.Append("accessToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Se è https = true
            SameSite = SameSiteMode.Strict
        });

        // Altri codici di gestione dopo il salvataggio del token...
        // Ad esempio, potresti reindirizzare l'utente a un'altra pagina.

        return RedirectToPage("/auth/SignToFarm");
    }
}
