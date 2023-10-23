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
}
