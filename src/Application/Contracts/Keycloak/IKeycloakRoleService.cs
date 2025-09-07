using Domain.ValueObjects;
using FluentResults;

namespace Application.Contracts.Keycloak;

public interface IKeycloakRoleService
{
    // Validates if the given role exists in Keycloak and returns the corresponding Role object
    Task<Result<Role>> RoleExistsAsync(string roleName, CancellationToken ct = default);

    Task<Result<IReadOnlyList<Role>>> GetValidRolesAsync(CancellationToken ct = default);
}