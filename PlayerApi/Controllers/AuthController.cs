using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PlayerApi.Models.Requests;
using PlayerApi.Models.Responses;

namespace PlayerApi.Controllers;

[ApiController]
[Route("api/tester")]
public class AuthController : ControllerBase
{
    private const string ExpectedUsername = "tester";
    private const string ExpectedPassword = "tester123";
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Authenticate a tester and return a JWT token.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <returns>JWT bearer token.</returns>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest? request)
    {
        if (request is null)
        {
            return BadRequest(new ErrorResponse("Request body is required."));
        }

        if (!string.Equals(request.Username, ExpectedUsername, StringComparison.Ordinal) ||
            !string.Equals(request.Password, ExpectedPassword, StringComparison.Ordinal))
        {
            return Unauthorized();
        }

        var jwtKey = _configuration["Jwt:Key"] ?? "dev-secret-key-32-chars-minimum!!";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new LoginResponse(tokenString));
    }
}
