using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GrKouk.Web.ERP.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController :ControllerBase
{
     private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;
    private static Dictionary<string, string> _refreshTokens = new();

    public AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByNameAsync(model.Username);
        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
        {
            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);
            var refreshToken = GenerateRefreshToken();

            _refreshTokens[user.Id] = refreshToken; // Store refresh token for the user

            return Ok(new { Token = token, RefreshToken = refreshToken, Roles = roles });
        }
        return Unauthorized("Invalid credentials");
    }

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] RefreshRequest request)
    {
        var principal = GetPrincipalFromExpiredToken(request.Token);
        if (principal == null) return Unauthorized("Invalid token.");

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId) || !_refreshTokens.ContainsKey(userId) || _refreshTokens[userId] != request.RefreshToken)
        {
            return Unauthorized("Invalid refresh token.");
        }
        var roles = principal.FindFirst(ClaimTypes.Role)?.Value;
        var newToken = GenerateJwtToken(new IdentityUser { Id = userId}, new List<string> { roles});
        var newRefreshToken = GenerateRefreshToken();

        _refreshTokens[userId] = newRefreshToken; // Update refresh token

        return Ok(new { Token = newToken, RefreshToken = newRefreshToken });
    }

    private string GenerateJwtToken(IdentityUser user, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationMinutes"])),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false // Allow expired tokens
        };

        var principal = tokenHandler.ValidateToken(token, validationParams, out SecurityToken securityToken);
        return securityToken is JwtSecurityToken jwtSecurityToken && jwtSecurityToken.Header.Alg == SecurityAlgorithms.HmacSha256
            ? principal
            : null;
    }
}

public class LoginModel
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class RefreshRequest
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
}
