using Application.Contracts.Keycloak;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Common;

namespace Web.API.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
public sealed class RolesController : ControllerBase
{
    private readonly IKeycloakRoleService _keycloakRoleService;

    public RolesController(IKeycloakRoleService keycloakRoleService)
    {
        _keycloakRoleService = keycloakRoleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles(CancellationToken ct = default)
    {
        var roles = await _keycloakRoleService.GetValidRolesAsync(ct);
        if (roles.IsFailed)
            return roles.ToProblemResponse(this, StatusCodes.Status404NotFound);

        return Ok(roles.Value);
    }
}