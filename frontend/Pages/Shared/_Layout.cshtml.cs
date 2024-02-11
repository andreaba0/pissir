namespace frontend.Pages
{
    public class _LayoutModel : PageModel
    {
        public async Task<IActionResult> OnGet()
        {
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return Page();

                // Chiamata alle API per ottenere i dati
                ApiReq.utente = await ApiReq.GetUserDataFromApi(HttpContext);

                return Page();
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
            


            return Page();           
        }
    }
}