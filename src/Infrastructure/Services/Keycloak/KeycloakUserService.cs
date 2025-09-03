using System.Net.Http.Headers;
using System.Text.Json;
using Application.Contracts.Keycloak;
using Application.DTOs.Users;
using FluentResults;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Keycloak;

internal sealed class KeycloakUserService : IKeycloakUserService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly IKeycloakService _keycloakService;
    
    public KeycloakUserService(IHttpClientFactory httpClient, IConfiguration config, IKeycloakService keycloakService)
    {
        _httpClient = httpClient.CreateClient(nameof(KeycloakUserService));
        _config = config;
        _keycloakService = keycloakService;
    }
    
    public async Task<Result<bool>> UserExistsAsync(string username, CancellationToken ct = default)
    {
        var accessToken = await _keycloakService.GetAccessTokenAsync(ct);
        if (accessToken.IsFailed)
            return Result.Fail<bool>(accessToken.Errors);
        
        var request = new HttpRequestMessage
        (
            HttpMethod.Get, 
            $"http://localhost:8081/auth/admin/realms/{_config["Keycloak:Realm"]}/users?username={username}"
        );
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value);
        
        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return Result.Fail<bool>($"Failed to check user existence. Status code: {response.StatusCode}");
        
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var users = json.RootElement.EnumerateArray();
        return Result.Ok(users.Any());
    }
    
    public async Task<Result<UserResponse>> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var accessToken = await _keycloakService.GetAccessTokenAsync(ct);
        if (accessToken.IsFailed)
            return Result.Fail<UserResponse>(accessToken.Errors);
        
        var request = new HttpRequestMessage
        (
            HttpMethod.Get, 
            $"http://localhost:8081/auth/admin/realms/{_config["Keycloak:Realm"]}/users/{userId}"
        );
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value);
        
        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return Result.Fail<UserResponse>($"Failed to retrieve user. Status code: {response.StatusCode}");
            
        
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        bool isEnabled = json.RootElement.GetProperty("enabled").GetBoolean();
        if (!isEnabled) // treating a disabled user as non-existent
            return Result.Fail<UserResponse>("User is disabled.");
        
        var keycloakUserId = Guid.Parse(json.RootElement.GetProperty("id").GetString()!);
        if (keycloakUserId == Guid.Empty)
            return Result.Fail<UserResponse>("User not found.");
        
        var username = json.RootElement.GetProperty("username").GetString();
        if (string.IsNullOrEmpty(username))
            return Result.Fail<UserResponse>("Username is missing.");
        
        var user = new UserResponse
        {
            ID = keycloakUserId,
            Username = username,
        };
        
        return Result.Ok(user);
    }
    
    public async Task<Result> RevokeUserSessionAsync(Guid userId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return Result.Fail("User ID is invalid");

        var accessToken = await _keycloakService.GetAccessTokenAsync(ct);
        if (accessToken.IsFailed)
            return Result.Fail(accessToken.Errors);
        
        var getSessionsRequest = new HttpRequestMessage
        (
            HttpMethod.Get, 
            $"http://localhost:8081/auth/admin/realms/{_config["Keycloak:Realm"]}/users/{userId}/sessions"
        );
        getSessionsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value);

        var getSessionsResponse = await _httpClient.SendAsync(getSessionsRequest, ct);
        if (!getSessionsResponse.IsSuccessStatusCode)
            return Result.Fail($"Failed to get sessions for user {userId}");

        var sessionsJson = JsonDocument.Parse(await getSessionsResponse.Content.ReadAsStringAsync(ct));
        // revoking all active sessions
        foreach(var session in sessionsJson.RootElement.EnumerateArray())
        {
            var sessionId = session.GetProperty("id").GetString();
            if (string.IsNullOrEmpty(sessionId))
                continue;

            var logoutRequest = new HttpRequestMessage
            (
                HttpMethod.Delete, 
                $"http://localhost:8081/auth/admin/realms/{_config["Keycloak:Realm"]}/sessions/{sessionId}"
            );
            logoutRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value);

            var logoutResponse = await _httpClient.SendAsync(logoutRequest, ct);
            if (!logoutResponse.IsSuccessStatusCode)
                return Result.Fail($"Failed to revoke session {sessionId} for user {userId}");
        }
        
        return Result.Ok();
    }
}