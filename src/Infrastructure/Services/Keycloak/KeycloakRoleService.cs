using System.Net.Http.Headers;
using System.Text.Json;
using Application.Contracts.Keycloak;
using Domain.ValueObjects;
using FluentResults;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services.Keycloak;

internal sealed class KeycloakRoleService : IKeycloakRoleService
{
    private readonly IKeycloakService _keycloakService;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "KeycloakRoles";

    public KeycloakRoleService(IKeycloakService keycloakService, IHttpClientFactory httpClient, IMemoryCache cache)
    {
        _keycloakService = keycloakService;
        _httpClient = httpClient.CreateClient(nameof(KeycloakRoleService));
        _cache = cache;
    }
    

    public async Task<Result<Role>> RoleExistsAsync(string roleName, CancellationToken ct = default)
    {
        if(_cache.TryGetValue(CacheKey, out IReadOnlyList<Role>? cachedRoles) && cachedRoles != null)
        {
            var cachedRole = cachedRoles.FirstOrDefault(r => r.Key.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            if (cachedRole != null)
                return Result.Ok(cachedRole);
        }
        var keycloakRoles = await GetValidRolesAsync(ct);
        var keycloakRole = keycloakRoles.Value.FirstOrDefault(r => r.Key.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        if (keycloakRole is null)
            return Result.Fail<Role>($"Role {roleName} does not exist.");

        var roleResult = Role.Create(keycloakRole.Key, keycloakRole.DisplayName);
        return roleResult;
    }

    private sealed record KeycloakRole(string Name);
    
    
    public async Task<Result<IReadOnlyList<Role>>> GetValidRolesAsync(CancellationToken ct = default)
    {
        var accessToken = await _keycloakService.GetAccessTokenAsync(ct);
        if (accessToken.IsFailed)
            return Result.Fail<IReadOnlyList<Role>>(accessToken.Errors);
        
        var getClientUUUIDReq = new HttpRequestMessage
        (
            HttpMethod.Get,
            "http://web.keycloak:8081/auth/admin/realms/BoardisRealm/clients?clientId=boardis-api"
        );
        getClientUUUIDReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value);
        
        var uuuidResponse = await _httpClient.SendAsync(getClientUUUIDReq, ct);
        if (!uuuidResponse.IsSuccessStatusCode)
        {
            var errorContent = await uuuidResponse.Content.ReadAsStringAsync(ct);
            await Console.Error.WriteLineAsync($"Failed to fetch UUUID from Keycloak: {uuuidResponse.StatusCode}, {errorContent}");
            return Result.Fail<IReadOnlyList<Role>>("Failed to fetch client UUUID from Keycloak");
        }
        var uuuidJson = await uuuidResponse.Content.ReadAsStringAsync(ct);
        var uuuidDoc = JsonDocument.Parse(uuuidJson);
        var uuuid = uuuidDoc.RootElement[0].GetProperty("id").GetString();
        
        var getReq = new HttpRequestMessage
        (
            HttpMethod.Get,
            $"http://web.keycloak:8081/auth/admin/realms/BoardisRealm/clients/{uuuid}/roles"
        );
        getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value);
        
        var response = await _httpClient.SendAsync(getReq, ct);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            await Console.Error.WriteLineAsync($"Failed to fetch roles from Keycloak: {response.StatusCode}, {errorContent}");
            return Result.Fail<IReadOnlyList<Role>>("Failed to fetch roles from Keycloak");
        }

        var json = await response.Content.ReadAsStringAsync(ct);
        var keycloakRoles = JsonSerializer.Deserialize<List<KeycloakRole>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var roles = keycloakRoles?
            .Select(r => Role.Create(r.Name.ToLowerInvariant(), r.Name).Value)
            .ToList() ?? new List<Role>();

        _cache.Set(CacheKey, roles, TimeSpan.FromHours(1));
        return roles;
    }
}