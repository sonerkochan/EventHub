using Microsoft.Playwright;
using EventHub.Tests.E2E;

namespace EventHub.Tests.E2E.Client;

[Trait("Category", "E2E")]
public class BuyTicketE2ETests
{
    private const string BaseUrl = "https://staging-eventhub.tryasp.net";

    private const string OrganizerEmail = "organizer@test.com";
    private const string OrganizerPassword = "Organizer123!";

    private const string ClientEmail = "client@test.com";
    private const string ClientPassword = "Client123!";

    [Fact]
    public async Task Client_CanBuyTicketDirectly()
    {
        var unique = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var eventName = $"BuyTicketTest_{unique}";

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });

        // Organizer creates and publishes the event

        var organizerContext = await CreateContextAsync(browser);
        var organizerPage = await organizerContext.NewPageAsync();

        await LoginAsync(organizerPage, OrganizerEmail, OrganizerPassword);

        await Organizer.CreateEventE2ETests.CreateEventAsync(
            organizerPage,
            eventName,
            "E2E събитие за закупуване на билет",
            startOffset: TimeSpan.FromDays(20),
            endOffset: TimeSpan.FromDays(20) + TimeSpan.FromHours(3),
            totalTickets: "50",
            basePrice: "10",
            coverImageUrl: "https://placehold.co/800x400");

        await organizerContext.CloseAsync();

        // publishes the event
        var adminContext = await CreateContextAsync(browser);
        var adminPage = await adminContext.NewPageAsync();
        await LoginAsync(adminPage, "admin", "Admin123!");

        await adminPage.GotoAsync(
            $"{BaseUrl}/Admin/Events/Index",
            new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForPageReadyAsync(adminPage);

        await AdminEventPublishHelper.PublishEventAndWaitAsync(adminPage, eventName);
        await adminContext.CloseAsync();

        // Client buys a ticket

        var clientContext = await CreateContextAsync(browser);
        var clientPage = await clientContext.NewPageAsync();

        await LoginAsync(clientPage, ClientEmail, ClientPassword);

        await clientPage.GotoAsync(
            $"{BaseUrl}/Client/Events/Index",
            new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForPageReadyAsync(clientPage);

        await ExpectVisibleTextWithReloadAsync(clientPage, eventName);

        var eventCard = clientPage.Locator(".eh-event-listing-card", new() { HasTextString = eventName }).First;
        await eventCard.Locator("a:has-text('View Details')").ClickAsync();
        await WaitForPageReadyAsync(clientPage);

        await clientPage.Locator("a:has-text('Get Tickets')").First.ClickAsync();
        await WaitForPageReadyAsync(clientPage);

        var quantityInput = clientPage.Locator("input[name='quantity']");
        await quantityInput.FillAsync("1");

        var buyForm = clientPage.Locator("form[action*='BuyDirect']").First;
        await buyForm.Locator("button:has-text('Reserve (Pay Later)'), button:has-text('Get Free Tickets'), button:has-text('Reserve')").First.ClickAsync();
        await WaitForPageReadyAsync(clientPage);

        await clientPage.WaitForURLAsync("**/Client/Tickets**", new() { Timeout = 15_000 });

        await ExpectVisibleTextAsync(clientPage, eventName);

        await clientContext.CloseAsync();
    }

    private static async Task<IBrowserContext> CreateContextAsync(IBrowser browser)
    {
        return await browser.NewContextAsync(new()
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = 1366, Height = 768 }
        });
    }

    private static async Task LoginAsync(IPage page, string username, string password)
    {
        await page.GotoAsync(
            $"{BaseUrl}/User/Login",
            new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 30_000 });

        await WaitForPageReadyAsync(page);

        await page.Locator(
                "input[name='Username'], input[name='UserName'], #Username, #UserName, input[type='text']")
            .First
            .FillAsync(username);

        await page.Locator(
                "input[name='Password'], #Password, input[type='password']")
            .First
            .FillAsync(password);

        await page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

        await WaitForPageReadyAsync(page);
    }

    private static async Task ExpectVisibleTextAsync(IPage page, string text)
    {
        await page.GetByText(text)
            .First
            .WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15_000
            });
    }

    private static async Task ExpectVisibleTextWithReloadAsync(IPage page, string text)
    {
        try
        {
            await ExpectVisibleTextAsync(page, text);
        }
        catch (TimeoutException)
        {
            await page.ReloadAsync(new() { WaitUntil = WaitUntilState.DOMContentLoaded });
            await WaitForPageReadyAsync(page);
            await ExpectVisibleTextAsync(page, text);
        }
    }

    private static async Task WaitForPageReadyAsync(IPage page)
    {
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        try
        {
            await page.WaitForLoadStateAsync(
                LoadState.NetworkIdle,
                new() { Timeout = 10_000 });
        }
        catch
        {
        }
    }
}
