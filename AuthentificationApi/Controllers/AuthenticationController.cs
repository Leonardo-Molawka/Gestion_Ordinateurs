using Microsoft.AspNetCore.Mvc;

namespace AuthentificationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login()
    {
        return StatusCode(
            StatusCodes.Status410Gone,
            "Cette connexion n'est plus utilisée. Connectez-vous via Keycloak depuis l'application cliente.");
    }
}
