using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GrKouk.Web.ERP.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;

    // In-memory store for refresh tokens (replace with DB storage in production)
    private static Dictionary<string, string> _refreshTokens = new Dictionary<string, string>();

    public AuthController(ApiDbContext context, UserManager<IdentityUser> userManager, IConfiguration configuration)
    {
        _context = context;
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
       // _refreshTokens[user.Id] = refreshToken;
        var tokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            IsUsed = false,
            IsRevoked = false,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            CreatedDate = DateTime.UtcNow,
            CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString()

        };
        _context.RefreshTokens.Add(tokenEntity);
        await _context.SaveChangesAsync();

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
        //for debugging purposes
        // foreach (var claim in principal.Claims)
        // {
        //     Console.WriteLine($"{claim.Type} => {claim.Value}");
        // }

        var userId = principal.FindFirst("UserId")?.Value;
        //-------------
        var oldToken =  _context.RefreshTokens
            .FirstOrDefault(t => 
                t.Token == request.RefreshToken  
                 && t.UserId == userId  
                 && !t.IsUsed 
                 && !t.IsRevoked 
                 && t.ExpiryDate > DateTime.UtcNow
                );

        if (oldToken == null)
        {
            return BadRequest("Invalid refresh token");
        }

        // Mark old token as used
        oldToken.IsUsed = true;
        _context.SaveChanges();

        //------------
        // Validate refresh token
        // if (string.IsNullOrEmpty(userId) || !_refreshTokens.ContainsKey(userId) || _refreshTokens[userId] != request.RefreshToken)
        //     return BadRequest("Invalid refresh token");

        // Generate new tokens
        var user = _userManager.Users.SingleOrDefault(u => u.Id == userId);
        if (user == null) return Unauthorized();

        var roles = _userManager.GetRolesAsync(user).Result;
        var newAccessToken = GenerateJwtToken(user, roles);
        var newRefreshToken = GenerateRefreshToken();

        // Update refresh token
        var newTokenRecord = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = userId,
            IsUsed = false,
            IsRevoked = false,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            CreatedDate = DateTime.UtcNow,
            CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString()
        };

        _context.RefreshTokens.Add(newTokenRecord);
        _context.SaveChanges();

        
        //_refreshTokens[userId] = newRefreshToken;

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
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("UserId", user.Id)
            
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
        var expirationMinutes = int.TryParse(jwtSettings["ExpirationMinutes"], out var configMinutes) ? configMinutes : 60;
        var token = new JwtSecurityToken(issuer, audience, claims, expires: DateTime.UtcNow.AddMinutes(expirationMinutes), signingCredentials: creds);

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
    
    
    /// <summary>
    /// ## Site-Wide Logout (Revoke All Tokens for a User)
    /// To implement a complete logout (revoking all existing refresh tokens for a user),
    /// create a method that marks all their tokens as revoked. For example:
    /// </summary>
    /// <returns></returns>
    [HttpPost("revokeAll")]
    [Authorize]
    public async Task<IActionResult> RevokeAllTokens()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var userTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in userTokens)
        {
            token.IsRevoked = true;
        }

        await _context.SaveChangesAsync();

        return Ok("All tokens revoked for this user.");
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
