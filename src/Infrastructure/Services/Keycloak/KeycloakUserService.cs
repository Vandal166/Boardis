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

    private enum QueryType
    {
        UserId,
        Username,
        Email
    }
    
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
            $"http://web.keycloak:8081/auth/admin/realms/{_config["Keycloak:Realm"]}/users?username={username}&exact=true"
        );
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value);
        
        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return Result.Fail<bool>($"Failed to check user existence. Status code: {response.StatusCode}");
        
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var users = json.RootElement.EnumerateArray();
        return Result.Ok(users.Count() == 1);
    }

    private async Task<Result<UserResponse>> GetUserResponseAsync(string identifier, QueryType type, CancellationToken ct = default)
    {
        var accessToken = await _keycloakService.GetAccessTokenAsync(ct);
        if (accessToken.IsFailed)
            return Result.Fail<UserResponse>(accessToken.Errors);
        
        var request = new HttpRequestMessage
        (
            HttpMethod.Get, 
            $"http://web.keycloak:8081/auth/admin/realms/{_config["Keycloak:Realm"]}/users" + (type == QueryType.UserId ? $"/{identifier}" : $"?{type}={identifier}&exact=true")
        );
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value);
        
        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return Result.Fail<UserResponse>($"Failed to retrieve user. Status code: {response.StatusCode}");
            
        
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var root = json.RootElement;
        if (root.ValueKind == JsonValueKind.Array) // when querying by username or email the result is an array not an single object
        {
            if (root.GetArrayLength() == 0)
                return Result.Fail<UserResponse>("User not found.");

            root = root[0];
        }

        bool isEnabled = root.GetProperty("enabled").GetBoolean();
        if (!isEnabled)
            return Result.Fail<UserResponse>("User is disabled.");

        var keycloakUserId = Guid.Parse(root.GetProperty("id").GetString()!);
        if (keycloakUserId == Guid.Empty)
            return Result.Fail<UserResponse>("User not found.");

        var username = root.GetProperty("username").GetString();
        if (string.IsNullOrEmpty(username))
            return Result.Fail<UserResponse>("Username is missing.");
        
        var email = root.GetProperty("email").GetString();
        if (string.IsNullOrEmpty(email))
            return Result.Fail<UserResponse>("Email is missing.");

        var user = new UserResponse
        {
            Id = keycloakUserId,
            Username = username,
            Email = email
        };

        return Result.Ok(user);
    }
    
    public async Task<Result<UserResponse>> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return Result.Fail<UserResponse>("User ID is invalid");
        
        return await GetUserResponseAsync(userId.ToString(), QueryType.UserId, ct);
    }

    public async Task<Result<UserResponse>> GetUserByNameAsync(string username, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(username) || username.Length > 50 || username.Length < 3)
            return Result.Fail<UserResponse>("Username is invalid");
        
        return await GetUserResponseAsync(username, QueryType.Username, ct);
    }

    public async Task<Result<UserResponse>> GetUserByEmailAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(email) || email.Length > 254 || email.Length < 4 || !email.Contains("@"))
            return Result.Fail<UserResponse>("Email is invalid");
        
        return await GetUserResponseAsync(email, QueryType.Email, ct);
    }

    //TODO into its own service
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
            $"http://web.keycloak:8081/auth/admin/realms/{_config["Keycloak:Realm"]}/users/{userId}/sessions"
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
                $"http://web.keycloak:8081/auth/admin/realms/{_config["Keycloak:Realm"]}/sessions/{sessionId}"
            );
            logoutRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value);

            var logoutResponse = await _httpClient.SendAsync(logoutRequest, ct);
            if (!logoutResponse.IsSuccessStatusCode)
                return Result.Fail($"Failed to revoke session {sessionId} for user {userId}");
        }
        
        return Result.Ok();
    }
}