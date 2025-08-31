using Application.DTOs.Users;
using FluentResults;

namespace Application.Contracts.Keycloak;

public interface IKeycloakUserService
{
    Task<Result<bool>> UserExistsAsync(string username, CancellationToken ct = default);
    Task<Result<UserResponse>> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
}