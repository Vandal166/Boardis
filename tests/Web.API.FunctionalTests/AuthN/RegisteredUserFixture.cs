using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace Web.API.FunctionalTests.AuthN;

public class RegisteredUserFixture : IAsyncLifetime
{
    private readonly IServiceScope _scope;
    
    public string Email { get; private set; }
    public string Password { get; private set; }
    public string Username => Email?.Split('@')[0];

    public RegisteredUserFixture() { }
    
    // Registers an actual user in Keycloak via Playwright
    // This is not a mock, the tests are using the real Keycloak instance.
    public async Task InitializeAsync()
    {
        Email = $"testuser{Guid.NewGuid().ToString().Substring(0, 8)}@example.com";
        Password = Email;
        
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true, // change to false to see the browser
        });
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });
        var page = await context.NewPageAsync();
        var frontendUrl = "http://localhost/";
        
        await page.GotoAsync(frontendUrl);
        await page.ClickAsync("text=Register");
        await page.FillAsync("input[name='username']", Username);
        await page.FillAsync("input[name='email']", Email);
        await page.FillAsync("input[name='password']", Password);
        await page.FillAsync("input[name='password-confirm']", Password);
        await page.ClickAsync("input[type='submit']");
        await page.WaitForSelectorAsync($"text={Username}", new PageWaitForSelectorOptions { Timeout = 5000 });

        await browser.CloseAsync();
    }

    public async Task DisposeAsync()
    {
        _scope?.Dispose();
        await Task.CompletedTask;
    }
}