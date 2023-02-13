using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tweetbook.Data;
using Tweetbook.Domain;
using Tweetbook.Options;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace Tweetbook.Services;
public class IdentityService : IIdentityService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly TokenValidationParameters _validationParameters;
    private readonly DataContext _context;
    private readonly JwtSettings _jwtSettings;

    public IdentityService(
        UserManager<IdentityUser> userManager,
        TokenValidationParameters validationParameters,
        DataContext context,
        JwtSettings jwtSettings)
    {
        _userManager = userManager;
        _validationParameters = validationParameters;
        _context = context;
        _jwtSettings = jwtSettings;
    }

    public async Task<AuthenticationResult> RegisterAsync(string email, string password)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser != null)
        {
            return new AuthenticationResult
            {
                Errors = new[] { "User with this email address already exists." }
            };
        }

        var newUserId = Guid.NewGuid();

        var newUser = new IdentityUser
        {
            Id = newUserId.ToString(),
            Email = email,
            UserName = email
        };

        var createdUser = await _userManager.CreateAsync(newUser, password);

        if (!createdUser.Succeeded)
        {
            return new AuthenticationResult
            {
                Errors = createdUser.Errors.Select(a => a.Description)
            };
        }

        await _userManager.AddClaimAsync(newUser, new Claim("tags.view", "true"));

        return await GenerateAuthenticationUserResultForUserAsync(newUser);
    }

    public async Task<AuthenticationResult> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return new AuthenticationResult
            {
                Errors = new[] { "User does not exist." }
            };
        }

        var userHasValidPassword = await _userManager.CheckPasswordAsync(user, password);

        if (!userHasValidPassword)
        {
            return new AuthenticationResult
            {
                Errors = new[] { "User/password combination is wrong" }
            };
        }

        return await GenerateAuthenticationUserResultForUserAsync(user);
    }

    private async Task<AuthenticationResult> GenerateAuthenticationUserResultForUserAsync(IdentityUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

        var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("id", user.Id)
            };

        var userClaims = await _userManager.GetClaimsAsync(user);

        claims.AddRange(userClaims);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(_jwtSettings.TokenLifetime),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            JwtId = token.Id,
            UserId = user.Id,
            CreationDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddMonths(6)
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return new AuthenticationResult
        {
            Success = true,
            Token = tokenHandler.WriteToken(token),
            RefreshToken = refreshToken.Token
        };
    }

    public async Task<AuthenticationResult> RefreshTokenAsync(string token, string refreshToken)
    {
        var validatedToken = GetPrincipalFromToken(token);

        if (validatedToken == null)
        {
            return new AuthenticationResult
            {
                Errors = new[] { "Invalid Token" }
            };
        }

        var expiryDate = long.Parse(
            validatedToken.Claims
            .Single(a => a.Type == JwtRegisteredClaimNames.Exp).Value);

        var expiryDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddSeconds(expiryDate);

        if (expiryDateTimeUtc > DateTime.UtcNow)
        {
            return new AuthenticationResult
            {
                Errors = new[] { "This token hasn't expired yet" }
            };
        }

        var jti = validatedToken.Claims
            .Single(a => a.Type == JwtRegisteredClaimNames.Jti).Value;

        var storedRefreshToken = await _context
            .RefreshTokens.SingleOrDefaultAsync(a => a.Token == refreshToken);

        if (storedRefreshToken == null)
        {
            return new AuthenticationResult
            {
                Errors = new[] { "This refresh token does not exist." }
            };
        }

        if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
        {
            return new AuthenticationResult
            {
                Errors = new[] { "This refresh token has expired." }
            };
        }

        if (storedRefreshToken.Isvalidated)
        {
            return new AuthenticationResult
            {
                Errors = new[] { "This refresh token has been validated." }
            };
        }

        if (storedRefreshToken.Used)
        {
            return new AuthenticationResult
            {
                Errors = new[] { "This refresh token has been used." }
            };
        }

        if (storedRefreshToken.JwtId != jti)
        {
            return new AuthenticationResult
            {
                Errors = new[] { "This refresh token does not match this JWT" }
            };
        }

        storedRefreshToken.Used = true;
        _context.RefreshTokens.Update(storedRefreshToken);
        await _context.SaveChangesAsync();

        var user = await _userManager.FindByIdAsync(
            validatedToken.Claims.Single(a => a.Type == "id").Value);

        return await GenerateAuthenticationUserResultForUserAsync(user);
    }

    private ClaimsPrincipal GetPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token, _validationParameters,
                out var validatedToken);
            if (!IsJwtWithValidSecurityAlgorithm(validatedToken))
            {
                return null;
            }

            return principal;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
    {
        return (validatedToken is JwtSecurityToken jwtSecurityToken) &&
            jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature,
                StringComparison.CurrentCultureIgnoreCase);
    }
}
