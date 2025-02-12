using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GrKouk.Web.ERP.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;

    // In-memory store for refresh tokens (replace with DB storage in production)
    private static Dictionary<string, string> _refreshTokens = new Dictionary<string, string>();

    public AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            return Unauthorized("Invalid username or password");

        // Retrieve roles for the user
        var roles = await _userManager.GetRolesAsync(user);

        // Generate tokens
        var accessToken = GenerateJwtToken(user, roles);
        var refreshToken = GenerateRefreshToken();

        // Store the refresh token (replace with DB logic if necessary)
        _refreshTokens[user.Id] = refreshToken;

        return Ok(new
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] RefreshRequest request)
    {
        var principal = GetPrincipalFromExpiredToken(request.Token);
        if (principal == null) return BadRequest("Invalid access token or refresh token");

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Validate refresh token
        if (string.IsNullOrEmpty(userId) || !_refreshTokens.ContainsKey(userId) || _refreshTokens[userId] != request.RefreshToken)
            return BadRequest("Invalid refresh token");

        // Generate new tokens
        var user = _userManager.Users.SingleOrDefault(u => u.Id == userId);
        if (user == null) return Unauthorized();

        var roles = _userManager.GetRolesAsync(user).Result;
        var newAccessToken = GenerateJwtToken(user, roles);
        var newRefreshToken = GenerateRefreshToken();

        // Update refresh token
        _refreshTokens[userId] = newRefreshToken;

        return Ok(new
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        });
    }

    [HttpPost("revoke")]
    [Authorize]
    public IActionResult Revoke()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (_refreshTokens.ContainsKey(userId))
            _refreshTokens.Remove(userId);

        return NoContent();
    }

    private string GenerateJwtToken(IdentityUser user, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, claims, expires: DateTime.UtcNow.AddMinutes(60), signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
        var randomBytes = new byte[32];
        rngCryptoServiceProvider.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false, // Audience is optional
            ValidateIssuer = false, // Issuer is optional
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = false // Ignore expiration
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
        catch
        {
            return null;
        }
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
