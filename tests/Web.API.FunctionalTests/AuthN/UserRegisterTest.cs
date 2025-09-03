using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;

namespace Web.API.FunctionalTests.AuthN;


public class UserRegisterTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime, IDisposable
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IPage _page;
    private IBrowserContext _context;
    
    [Fact] // Not mocking, we are actually registering a user that gets saved in Keycloak_db
    public async Task RegisterUser_ShouldSucceedWithRedirectToLogin_WhenValidData()
    {
        var frontendUrl = "http://localhost/";
        var email = $"testuser{Guid.NewGuid().ToString().Substring(0, 8)}@example.com";
        var username = email.Split('@')[0];
        var pwd = email;

        await _page.GotoAsync(frontendUrl);
        
        await _page.ClickAsync("text=Register"); // Click on the Register link
        await _page.FillAsync("input[name='username']", username);
        await _page.FillAsync("input[name='email']", email);
        await _page.FillAsync("input[name='password']", pwd);
        await _page.FillAsync("input[name='password-confirm']", pwd);
        await _page.ClickAsync("input[type='submit']");
        
        await _page.WaitForSelectorAsync($"text={username}", new PageWaitForSelectorOptions { Timeout = 5000 });
        
       // Assert
       Assert.Equal(frontendUrl, _page.Url);
       Assert.Contains("Vite + React + TS", await _page.TitleAsync());
       Assert.Contains(username, await _page.ContentAsync());
       var cookies = await _context.CookiesAsync(); 
       Assert.Contains(cookies, c => c.Name == "KEYCLOAK_SESSION");
       Assert.Contains(cookies, c => c.Name == "KEYCLOAK_IDENTITY");
       Assert.Contains(cookies, c => c.Name == "KC_AUTH_SESSION_HASH");
       Assert.Contains(cookies, c => c.Name == "AUTH_SESSION_ID");
    }
    
    public async Task InitializeAsync()
    {
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