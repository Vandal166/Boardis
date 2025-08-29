using System.Security.Claims;
using Application.Contracts.User;
using Microsoft.AspNetCore.Http;

namespace Application.Services;

internal sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _http;
    
    public CurrentUser(IHttpContextAccessor http)
        => _http = http;
    
    private ClaimsPrincipal User => _http.HttpContext?.User ?? new ClaimsPrincipal();
    
    public bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;
    
    public Guid Id
    {
        get
        {
            var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
        }
    }
    
    public string Username => User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
    
    public IReadOnlyCollection<string> Roles => User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList().AsReadOnly();
}