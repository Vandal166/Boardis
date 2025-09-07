using Application.Contracts.Keycloak;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Common;

namespace Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IKeycloakUserService _keycloakUserService;

    public UsersController(IKeycloakUserService keycloakUserService)
    {
        _keycloakUserService = keycloakUserService;
    }
    
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken ct = default)
    {
        var userResult = await _keycloakUserService.GetUserByIdAsync(id, ct);
        if (userResult.IsFailed)
            return userResult.ToProblemResponse(this, StatusCodes.Status404NotFound);

        return Ok(userResult.Value);
    }

    [HttpGet("username/{username}")]
    public async Task<IActionResult> GetUserByUsername(string username, CancellationToken ct = default)
    {
        var userResult = await _keycloakUserService.GetUserByNameAsync(username, ct);
        if (userResult.IsFailed)
            return userResult.ToProblemResponse(this, StatusCodes.Status404NotFound);

        return Ok(userResult.Value);
    }
    
    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetUserByEmail(string email, CancellationToken ct = default)
    {
        var userResult = await _keycloakUserService.GetUserByEmailAsync(email, ct);
        if (userResult.IsFailed)
            return userResult.ToProblemResponse(this, StatusCodes.Status404NotFound);
        
        return Ok(userResult.Value);
        
    }
}