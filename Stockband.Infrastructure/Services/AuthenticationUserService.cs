using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Stockband.Application.Interfaces.Services;
using Stockband.Domain.Enums;
using Stockband.Domain.Exceptions;

namespace Stockband.Infrastructure.Services;

public class AuthenticationUserService:IAuthenticationUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfigurationHelperService _configurationHelperService;
    
    public AuthenticationUserService(
        IHttpContextAccessor httpContextAccessor,
        IConfigurationHelperService configurationHelperService)
    {
        _httpContextAccessor = httpContextAccessor;
        _configurationHelperService = configurationHelperService;
    }
    
    /// <summary>
    /// Generates a JWT token based on the provided user information.
    /// </summary>
    /// <param name="userId">The user ID associated with the token.</param>
    /// <param name="username">The username associated with the token.</param>
    /// <param name="email">The email associated with the token.</param>
    /// <param name="role">The role associated with the token.</param>
    /// <returns>The generated JWT token.</returns>
    public string GenerateJwtToken(string userId, string username, string email, string role)
    {
        JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

        string jwtKey = _configurationHelperService.GetJwtKey();

        string jwtAudience = _configurationHelperService.GetJwtAudience();

        string jwtIssuer = _configurationHelperService.GetJwtIssuer();

        double jwtExpires = _configurationHelperService.GetJwtExpires();
        
        byte[] keyBytes = Encoding.UTF8.GetBytes(jwtKey);
        SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new (ClaimTypes.NameIdentifier, userId),
                new (ClaimTypes.Name, username),
                new (ClaimTypes.Email, email),
                new (ClaimTypes.Role, role)
            }),
            Expires = DateTime.Now.AddMinutes(jwtExpires),
            Audience = jwtAudience,
            Issuer = jwtIssuer,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
        };
        
        SecurityToken token = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
        string tokenString = jwtSecurityTokenHandler.WriteToken(token);

        return tokenString;
    }

    /// <summary>
    /// Adds a JWT token as a cookie to the current HttpContext.
    /// </summary>
    /// <param name="jwtToken">The JWT token to be added as a cookie.</param>
    /// <exception cref="ObjectNotFound">Thrown when the HttpContext is null.</exception>
    public void AddJwtCookie(string jwtToken)
    {
        string cookieName = _configurationHelperService.GetCookieName();

        double cookieExpires = _configurationHelperService.GetCookieExpires();
        
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new ObjectNotFound(typeof(HttpContext));
        }
        
        httpContext.Response.Cookies.Append(cookieName, jwtToken, new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.Now.AddMinutes(cookieExpires),
            SameSite = SameSiteMode.None
        });
    }

    /// <summary>
    /// Clears the JWT cookie from the current HttpContext.
    /// </summary>
    /// <exception cref="ObjectNotFound">Thrown when the HttpContext is null.</exception>
    public void ClearJwtCookie()
    {
        string cookieName = _configurationHelperService.GetCookieName();

        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new ObjectNotFound(typeof(HttpContext));
        }
        
        httpContext.Response.Cookies.Delete(cookieName);
    }

    /// <summary>
    /// Retrieves the current user ID.
    /// </summary>
    /// <returns>The current user ID.</returns>
    /// <exception cref="ObjectNotFound">Thrown if the claim is null.</exception>
    /// <exception cref="FormatException">Thrown if the user ID could not be parsed.</exception>
    public int GetUserId()
    {
        Claim? claim = GetHttpContext().User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        if (claim == null)
        {
            throw new ObjectNotFound(typeof(Claim));
        }
        
        bool success = int.TryParse(claim.Value, out int parsedId);
        if (!success)
        {
            throw new FormatException();
        }

        return parsedId;
    }
    
    /// <summary>
    /// Retrieves all roles associated with the user.
    /// </summary>
    /// <returns>A collection of user roles.</returns>
    public IEnumerable<string> GetRoles()
    {
        return GetHttpContext()
            .User
            .Claims
            .Where(x => x.Type == ClaimTypes.Role)
            .Select(x => x.Value);
    }

    /// <summary>
    /// Verifies whether the provided user ID belongs to an admin or is equal to the claim ID.
    /// </summary>
    /// <param name="userId">The user ID to be verified.</param>
    /// <returns><c>true</c> if the user is authorized; otherwise, <c>false</c>.</returns>
    public bool IsAuthorized(int userId) =>
        GetUserId() == userId || GetHttpContext().User.IsInRole(UserRole.Admin.ToString());
    
    private HttpContext GetHttpContext()
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new ObjectNotFound(typeof(HttpContext));
        }
        return httpContext;
    }
}