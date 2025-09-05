using System.Text.Json;
using Application.Contracts.Keycloak;
using FluentResults;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Keycloak;

internal sealed class KeycloakService : IKeycloakService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    
    public KeycloakService(IHttpClientFactory httpClient, IConfiguration config)
    {
        _httpClient = httpClient.CreateClient(nameof(KeycloakService));
        _config = config;
    }
    
    
    public async Task<Result<string>> GetAccessTokenAsync(CancellationToken ct = default)
    {
        var request = new HttpRequestMessage
        (
            HttpMethod.Post, 
            //$"{_config["Keycloak:Authority"]}/protocol/openid-connect/token"
            "http://web.keycloak:8081/auth/realms/BoardisRealm/protocol/openid-connect/token"
        );
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", _config["Keycloak:AdminCliId"]! },
            { "client_secret", _config["Keycloak:AdminCliSecret"]! },
            { "grant_type", "client_credentials" }
        });
        request.Content = content;
        
        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return Result.Fail<string>($"Failed to get access token: {response.StatusCode}");
        
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var accessToken = json.RootElement.GetProperty("access_token").GetString();

        return accessToken switch
        {
            null => Result.Fail<string>("Failed to obtain access token"),
            _ => Result.Ok(accessToken)
        };
    }
}