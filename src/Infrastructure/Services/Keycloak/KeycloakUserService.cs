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
    
    public KeycloakUserService(HttpClient httpClient, IConfiguration config, IKeycloakService keycloakService)
    {
        _httpClient = httpClient;
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
            $"http://localhost:8081/admin/realms/{_config["Keycloak:Realm"]}/users?username={username}"
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
            $"http://localhost:8081/admin/realms/{_config["Keycloak:Realm"]}/users/{userId}"
        );
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value);
        
        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return Result.Fail<UserResponse>($"Failed to retrieve user. Status code: {response.StatusCode}");
            
        
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
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
}