using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("[controller]")]
public class SignInController : ControllerBase
{
    private readonly ILogger<SignInController> _logger;

    public SignInController(ILogger<SignInController> logger)
    {
        _logger = logger;
    }

    [HttpPost(Name = "SignIn")]
    public async Task<IActionResult> SignIn()
    {
        return Ok();
    }
}