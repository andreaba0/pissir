using frontend.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class SignInModel : PageModel
{
    // test
    string token = "XXXXXAAAAA";

    public IActionResult OnGet()
    {
        // Salvataggio del token come cookie
        Response.Cookies.Append("AccessToken", token, new CookieOptions
        {
            Path = "/",
            Expires = DateTime.Now.AddDays(1),
            HttpOnly = false,
            Secure = false,
        });

        return Page();
    }

    public async Task<IActionResult> OnGetCallbackAsync()
    {
        string? token = Request.Query["token"];
        
        // Verifica se il token è valido (es. presenza e formato)
        if (!string.IsNullOrEmpty(token))
        {
            Response.Cookies.Append("AccessToken", token, new CookieOptions
            {
                Path = "/",
                Expires = DateTime.Now.AddDays(1),
                HttpOnly = false,
                Secure = false,
            });

            // Ottieni il CF dell'utente loggato
            string? codFiscale = User.FindFirst("sub")?.Value;

            // Chiamata alle API per ottenere i dati
            if (codFiscale != null)
            {
                ApiReq.utente = await ApiReq.GetUserDataFromApi(codFiscale, HttpContext);
            }
            else
            {
                return BadRequest();
            }

            // Se è stato accettato nel sistema
            if (!string.IsNullOrEmpty(ApiReq.utente.Role) && !string.IsNullOrEmpty(ApiReq.utente.PartitaIva))
                return RedirectToPage("/DatiAccount");
            else
                return RedirectToPage("/auth/SignToFarm");
        }
        else
        {
            TempData["MessaggioErrore"] = "Qualcosa è andato storto. Autenticazione fallita.";
            return RedirectToPage();
        }
        

        TempData["MessaggioErrore"] = "Qualcosa è andato storto. Autenticazione fallita.";
        return RedirectToPage();
    }

}
