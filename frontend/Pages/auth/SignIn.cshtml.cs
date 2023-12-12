using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

public class SignInModel : PageModel
{
    private readonly IConfiguration _configuration;

    public SignInModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string? ClientId { get; set; }

    public void OnGet()
    {
        ClientId = _configuration["oauth:google:client_id"];
    }

    /*
    public void OnGet()
    {
        var token = Request.Query["token"];

        // Ora hai il token e puoi gestirlo come desideri, ad esempio, salvarlo come cookie.
        // Salva il token come cookie
        Response.Cookies.Append("accessToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Imposta su true se stai usando HTTPS
            SameSite = SameSiteMode.Strict
        });

        // Altri codici di gestione dopo il salvataggio del token...
        // Ad esempio, potresti reindirizzare l'utente a un'altra pagina.
    }
    */

}
