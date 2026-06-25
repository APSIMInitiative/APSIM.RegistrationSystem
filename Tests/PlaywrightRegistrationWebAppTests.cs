using Microsoft.Playwright;
using Tests.Utilities;

namespace Tests.RegistrationWebApp;

[CollectionDefinition(CollectionName)]
public sealed class PlaywrightUiCollection : ICollectionFixture<UiTestFixture>
{
    public const string CollectionName = "Playwright UI";
}

[Collection(PlaywrightUiCollection.CollectionName)]
public sealed class PlaywrightRegistrationWebAppTests(UiTestFixture fixture)
{
    [Fact]
    public async Task HomePage_RendersExpectedContent()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        IPage page = await context.NewPageAsync();

        await page.GotoAsync(fixture.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
        });

        string headingText = await page.Locator("h2").First.InnerTextAsync();
        Assert.Contains("Register and Download", headingText, StringComparison.OrdinalIgnoreCase);

        string? legacyLinkHref = await page.GetAttributeAsync("a[href='/validate']", "href");
        Assert.Equal("/validate", legacyLinkHref);
    }

    [Fact]
    public async Task RegisterPage_ShowsLicenceTableAndActions()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        IPage page = await context.NewPageAsync();

        await page.GotoAsync($"{fixture.BaseUrl}/register", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
        });

        string pageHeading = await page.Locator("h2").First.InnerTextAsync();
        Assert.Contains("Register for a Product Licence", pageHeading, StringComparison.OrdinalIgnoreCase);

        IReadOnlyList<string> headers = await page.Locator("#licence-decision-table th").AllInnerTextsAsync();
        Assert.Contains(headers, header => header.Contains("General Use Licence", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(headers, header => header.Contains("Special Use Licence - Type 1", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(headers, header => header.Contains("Special Use Licence - Type 2", StringComparison.OrdinalIgnoreCase));

        string? generalActionHref = await page.GetAttributeAsync("a[href='/validate']", "href");
        Assert.Equal("/validate", generalActionHref);
    }

    [Fact]
    public async Task ValidatePage_ShowsEmailVerificationForm()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        IPage page = await context.NewPageAsync();

        await page.GotoAsync($"{fixture.BaseUrl}/validate", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
        });

        string headingText = await page.Locator("h3").First.InnerTextAsync();
        Assert.Contains("Email Verification", headingText, StringComparison.OrdinalIgnoreCase);

        bool hasEmailInput = await page.Locator("#emailInput").CountAsync() == 1;
        Assert.True(hasEmailInput, "Expected validate form to include #emailInput.");

        string buttonText = await page.Locator("button[type='submit']").First.InnerTextAsync();
        Assert.Contains("Verify Email", buttonText, StringComparison.OrdinalIgnoreCase);
    }
}
