using Microsoft.Playwright;

namespace EventHub.Tests.E2E.Organizer;

[Trait("Category", "E2E")]
public class CreateEventE2ETests
{
    private const string BaseUrl = "https://staging-eventhub.tryasp.net";

    private const string OrganizerEmail = "organizer@test.com";
    private const string OrganizerPassword = "Organizer123!";

    [Fact]
    public async Task Organizer_CanCreateEvent()
    {
        var unique = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var eventName = $"TestEvent_{unique}";

        using var playwright = await Playwright.CreateAsync();

        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = false
        });

        var context = await CreateContextAsync(browser);
        var page = await context.NewPageAsync();

        await LoginAsync(page, OrganizerEmail, OrganizerPassword);

        await CreateEventAsync(
            page,
            eventName,
            "E2E автоматизиран тест – описание",
            startOffset: TimeSpan.FromDays(30),
            endOffset: TimeSpan.FromDays(30) + TimeSpan.FromHours(3),
            totalTickets: "100",
            basePrice: "25",
            coverImageUrl: "https://placehold.co/800x400");

        await ExpectVisibleTextAsync(page, eventName);

        await context.CloseAsync();
    }

    internal static async Task<IBrowserContext> CreateContextAsync(IBrowser browser)
    {
        return await browser.NewContextAsync(new()
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = 1366, Height = 768 }
        });
    }

    internal static async Task LoginAsync(IPage page, string username, string password)
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

    // Creates an event
    internal static async Task CreateEventAsync(
        IPage page,
        string eventName,
        string description,
        TimeSpan startOffset,
        TimeSpan endOffset,
        string totalTickets,
        string basePrice,
        string coverImageUrl)
    {
        await page.GotoAsync(
            $"{BaseUrl}/Organizer/Events/Create",
            new() { WaitUntil = WaitUntilState.DOMContentLoaded });

        await WaitForPageReadyAsync(page);

        await page.Locator("input[name='EventName']").FillAsync(eventName);

        var roomSelect = page.Locator("select[name='RoomId']");
        var firstRoomValue = await roomSelect.EvaluateAsync<string>(
            "select => [...select.options].find(o => o.value && o.value !== '')?.value ?? ''");

        if (string.IsNullOrEmpty(firstRoomValue))
            throw new InvalidOperationException(
                "No rooms found in the Room dropdown. Make sure the staging environment has at least one room configured.");

        await roomSelect.SelectOptionAsync(firstRoomValue);

        await page.Locator("textarea[name='Description']").FillAsync(description);

        var start = DateTime.UtcNow.Add(startOffset);
        var end = DateTime.UtcNow.Add(endOffset);
        var startStr = start.ToString("yyyy-MM-ddTHH:mm");
        var endStr = end.ToString("yyyy-MM-ddTHH:mm");

        await page.Locator("input[name='StartDateTime']").FillAsync(startStr);
        await page.Locator("input[name='EndDateTime']").FillAsync(endStr);

        await page.Locator("input[name='TotalTickets']").FillAsync(totalTickets);
        await page.Locator("input[name='BasePrice']").FillAsync(basePrice);

        await page.Locator("input[name='CoverImageUrl']").FillAsync(coverImageUrl);

        await page.GetByRole(AriaRole.Button, new() { Name = "Create Event" }).ClickAsync();

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
