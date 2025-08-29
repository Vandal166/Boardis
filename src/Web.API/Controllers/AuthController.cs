using Application.Contracts.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public AuthController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    [HttpGet("login")]
    public IActionResult Login(string returnUrl = "/")
    {
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(Callback), new { returnUrl }),
            IsPersistent = true
        }, OpenIdConnectDefaults.AuthenticationScheme);
    }
    
    [HttpGet("register")]
    public IActionResult Register(string returnUrl = "/")
    {
        var redirectUri = Url.Action(nameof(Login), "Auth", new { returnUrl }, protocol: Request.Scheme)?.ToLowerInvariant();
        var registrationUrl = "http://localhost:8081/realms/BoardisRealm/protocol/openid-connect/registrations" +
                              "?client_id=boardis-api" +
                              "&response_type=code" +
                              "&scope=openid profile email" +
                              $"&redirect_uri={Uri.EscapeDataString(redirectUri ?? "http://localhost:5185/api/auth/login")}";

        return Redirect(registrationUrl);
    }
    
    [HttpGet("callback")]
    public async Task<IActionResult> Callback(string returnUrl = "/")
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!result.Succeeded)
        {
            return StatusCode(500, new { Error = "Authentication failed.", Detail = result.Failure?.Message });
        }
        
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, result.Principal, result.Properties);

        var accessToken = result.Properties.GetTokenValue("access_token");
        if (string.IsNullOrEmpty(accessToken))
        {
            return BadRequest(new { Error = "No access token received." });
        }

        return Ok(new { AccessToken = accessToken, RedirectUrl = returnUrl });
    }

    [HttpGet("logout")]
    public IActionResult Logout(string returnUrl = "/")
    {
        return SignOut(new AuthenticationProperties
        {
            RedirectUri = returnUrl
        }, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
    }
    
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        Console.WriteLine($"{_currentUser.IsAuthenticated}");
        if (!User.Identity.IsAuthenticated)
        {
            return Unauthorized(new { Error = "Not authenticated." });
        }

        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(new { Claims = claims });
    }
    
    [HttpGet("access-denied")]
    public IActionResult AccessDenied()
    {
        return StatusCode(403, new { Error = "Access denied." });
    }
}