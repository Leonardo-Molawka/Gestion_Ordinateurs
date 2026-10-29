using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthentificationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IConfiguration configuration;
    private readonly ILogger<AuthenticationController> logger;

    public AuthenticationController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AuthenticationController> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.configuration = configuration;
        this.logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Login))
        {
            return BadRequest("Le login est obligatoire.");
        }

        var usersApi = httpClientFactory.CreateClient("UsersApi");
        HttpResponseMessage userResponse;
        try
        {
            var escapedLogin = Uri.EscapeDataString(request.Login.Trim());
            userResponse = await usersApi.GetAsync($"api/users/{escapedLogin}", cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Impossible de contacter l'API de gestion des utilisateurs.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "L'API utilisateurs est indisponible.");
        }

        using (userResponse)
        {
            if (userResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound("Cet utilisateur n'existe pas.");
            }

            if (!userResponse.IsSuccessStatusCode)
            {
                logger.LogError("L'API utilisateurs a répondu avec le statut {StatusCode}.", userResponse.StatusCode);
                return StatusCode(StatusCodes.Status502BadGateway, "La recherche de l'utilisateur a échoué.");
            }

            var user = await userResponse.Content.ReadFromJsonAsync<User>(cancellationToken: cancellationToken);
            if (user is null)
            {
                logger.LogError("L'API utilisateurs a renvoyé une réponse vide.");
                return StatusCode(StatusCodes.Status502BadGateway, "La réponse de l'API utilisateurs est invalide.");
            }

            var expiresAt = DateTime.UtcNow.AddMinutes(15);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return Ok(new AuthenticationResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAt));
        }
    }
}

public sealed class LoginRequest
{
    public string Login { get; set; } = string.Empty;
}

public sealed record AuthenticationResponse(string Token, DateTime ExpiresAt);
