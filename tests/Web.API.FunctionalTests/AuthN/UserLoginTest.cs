using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;

namespace Web.API.FunctionalTests.AuthN;

[Collection("RegisteredUser collection")]
public class UserLoginTest : IAsyncLifetime, IDisposable
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IPage _page;
    private IBrowserContext _context;

    private readonly RegisteredUserFixture _userFixture;

    public UserLoginTest(RegisteredUserFixture userFixture)
    {
        _userFixture = userFixture;
    }
    
    [Fact]
    public async Task LoginUser_ShouldSucceedWithRedirectToCompleteProfile_WhenValidData()
    {
        var frontendUrl = "http://localhost/";

        await _page.GotoAsync(frontendUrl);
        
        await _page.ClickAsync("text=Login"); // Click on the login link
        await _page.FillAsync("input[name='username']", _userFixture.Email);
        await _page.FillAsync("input[name='password']", _userFixture.Password);

        await _page.ClickAsync("input[type='submit']");
        var tokenResponse = await _page.WaitForResponseAsync(
            r => r.Url.Contains("/protocol/openid-connect/token") && r.Request.Method == "POST" && r.Status == 200,
            new PageWaitForResponseOptions { Timeout = 5000 }
        );

        await _page.WaitForSelectorAsync($"text={_userFixture.Username}", new PageWaitForSelectorOptions { Timeout = 5000 });
        
        // Assert
        var json = await tokenResponse.JsonAsync();
        Assert.True(json.GetValueOrDefault().TryGetProperty("access_token", out _));
        Assert.True(json.GetValueOrDefault().TryGetProperty("refresh_token", out _));
        Assert.True(json.GetValueOrDefault().TryGetProperty("id_token", out _));
        Assert.Equal("Bearer", json.GetValueOrDefault().GetProperty("token_type").GetString());
        Assert.True(json.GetValueOrDefault().TryGetProperty("expires_in", out _));
        Assert.True(json.GetValueOrDefault().TryGetProperty("scope", out _));
        
        Assert.Equal(frontendUrl, _page.Url);
        Assert.Contains("Vite + React + TS", await _page.TitleAsync());
        Assert.Contains(_userFixture.Username, await _page.ContentAsync());
        var cookies = await _context.CookiesAsync(); 
        Assert.Contains(cookies, c => c.Name == "KEYCLOAK_SESSION");
        Assert.Contains(cookies, c => c.Name == "KEYCLOAK_IDENTITY");
        Assert.Contains(cookies, c => c.Name == "KC_AUTH_SESSION_HASH");
        Assert.Contains(cookies, c => c.Name == "AUTH_SESSION_ID");
    }
    
    public async Task InitializeAsync()
    {
        await _userFixture.InitializeAsync();
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true, // change to false to see the browser
        });

        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });

        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    public void Dispose()
    {
        _page?.CloseAsync().Wait();
        _context?.CloseAsync().Wait();
    }
}