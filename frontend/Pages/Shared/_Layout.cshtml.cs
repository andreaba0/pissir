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

                // Richiesta API
                string data = await ApiReq.GetDataFromApi(HttpContext, "/profile");
                ApiReq.utente = JsonConvert.DeserializeObject<Utente>(data);

                return Page();
            }
            catch (Exception ex)
            {
                TempData["MessaggioErrore"] = ex.Message;
                return RedirectToPage("/Error");
            }
        }
    }
}