using FluentResults;

namespace Application.Contracts.Keycloak;

public interface IKeycloakService
{
    /// <summary>
    /// Retrieves an admin cli access token from Keycloak using client credentials.
    /// </summary>
    /// <returns>>The access token as a string.</returns>
    Task<Result<string>> GetAccessTokenAsync(CancellationToken ct = default);
}