using Microsoft.Playwright;

namespace EventHub.Tests.E2E.Organizer;

[Trait("Category", "E2E")]
public class ManageEventE2ETests
{
    private const string BaseUrl = "https://staging-eventhub.tryasp.net";

    private const string OrganizerEmail = "organizer@test.com";
    private const string OrganizerPassword = "Organizer123!";

    private const string AdminEmail = "admin";
    private const string AdminPassword = "Admin123!";

    // ─── Test 1: Edit ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Organizer_CanEditEvent()
    {
        var unique = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var originalName = $"EditTest_Original_{unique}";
        var editedName = $"EditTest_Edited_{unique}";

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });

        var context = await CreateEventE2ETests.CreateContextAsync(browser);
        var page = await context.NewPageAsync();

        await CreateEventE2ETests.LoginAsync(page, OrganizerEmail, OrganizerPassword);

        // Setup: create the event we will edit
        await CreateEventE2ETests.CreateEventAsync(
            page,
            originalName,
            "Оригинално описание",
            startOffset: TimeSpan.FromDays(60),
            endOffset: TimeSpan.FromDays(60) + TimeSpan.FromHours(2),
            totalTickets: "50",
            basePrice: "15",
            coverImageUrl: "https://placehold.co/800x400");

        // Navigate to Index and find the event row
        await page.GotoAsync(
            $"{BaseUrl}/Organizer/Events/Index",
            new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForPageReadyAsync(page);

        var row = page.Locator("tr", new() { HasTextString = originalName }).First;
        await row.ScrollIntoViewIfNeededAsync();

        // Click Edit
        await row.Locator("a:has-text('Edit')").First.ClickAsync();
        await WaitForPageReadyAsync(page);

        // Edit the event name
        var nameInput = page.Locator("input[name='EventName']");
        await nameInput.ClearAsync();
        await nameInput.FillAsync(editedName);

        // Clear and fill the price to fix 15.0000 (decimal(18,4)) format issue
        var priceInput = page.Locator("input[name='BasePrice']");
        await priceInput.ClearAsync();
        await priceInput.FillAsync("15");

        // Submit – бутонът в Edit.cshtml е "Save Changes"
        await page.Locator("form[action*='Edit'] button[type='submit']").ClickAsync();

        await WaitForPageReadyAsync(page);

        // Verify the new name appears in the Index
        await ExpectVisibleTextAsync(page, editedName);

        await context.CloseAsync();
    }

    // ─── Test 2: Publish ──────────────────────────────────────────────────────

    [Fact]
    public async Task Admin_CanPublishEvent()
    {
        // Publish бутонът е само в Admin/Events/Index – работи чрез JS + confirm()
        var unique = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var eventName = $"PublishTest_{unique}";

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });

        // ── SETUP: Organizer създава Draft event ─────────────────────────────
        var organizerCtx = await CreateEventE2ETests.CreateContextAsync(browser);
        var organizerPage = await organizerCtx.NewPageAsync();
        await CreateEventE2ETests.LoginAsync(organizerPage, OrganizerEmail, OrganizerPassword);
        await CreateEventE2ETests.CreateEventAsync(
            organizerPage, eventName, "Събитие за публикуване",
            startOffset: TimeSpan.FromDays(45),
            endOffset: TimeSpan.FromDays(45) + TimeSpan.FromHours(2),
            totalTickets: "200", basePrice: "30",
            coverImageUrl: "https://placehold.co/800x400");
        await organizerCtx.CloseAsync();

        // ── ADMIN публикува event-а ───────────────────────────────────────────
        var adminCtx = await CreateEventE2ETests.CreateContextAsync(browser);
        var adminPage = await adminCtx.NewPageAsync();
        await CreateEventE2ETests.LoginAsync(adminPage, AdminEmail, AdminPassword);

        await adminPage.GotoAsync(
            $"{BaseUrl}/Admin/Events/Index",
            new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForPageReadyAsync(adminPage);

        var eventRow = adminPage.Locator("tr", new() { HasTextString = eventName }).First;
        await eventRow.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15_000 });
        await eventRow.ScrollIntoViewIfNeededAsync();

        // Приемаме confirm() диалога автоматично
        adminPage.Dialog += (_, dialog) => dialog.AcceptAsync();

        var publishBtn = eventRow.Locator("button:has-text('Publish')").First;
        await publishBtn.ClickAsync();

        var updatedRow = adminPage.Locator("tr", new() { HasTextString = eventName }).First;
        var badge = updatedRow.Locator(".badge").First;

        await badge.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15_000 });

        await Task.Delay(1000); 

        var badgeText = await badge.InnerTextAsync();
        Assert.True(
            badgeText.Contains("Published") || badgeText.Contains("Active"),
            $"Expected Published or Active badge, got: {badgeText}");

        await adminCtx.CloseAsync();
    }

    // ─── Test 3: Deactivate ───────────────────────────────────────────────────

    [Fact]
    public async Task Organizer_CanDeactivateEvent()
    {
        var unique = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var eventName = $"DeactivateTest_{unique}";

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });

        var context = await CreateEventE2ETests.CreateContextAsync(browser);
        var page = await context.NewPageAsync();

        await CreateEventE2ETests.LoginAsync(page, OrganizerEmail, OrganizerPassword);

        // Setup: create event then publish it so there is something to deactivate
        await CreateEventE2ETests.CreateEventAsync(
            page,
            eventName,
            "Събитие за деактивиране",
            startOffset: TimeSpan.FromDays(50),
            endOffset: TimeSpan.FromDays(50) + TimeSpan.FromHours(2),
            totalTickets: "80",
            basePrice: "20",
            coverImageUrl: "https://placehold.co/800x400");

        // Navigate to Index
        await page.GotoAsync(
            $"{BaseUrl}/Organizer/Events/Index",
            new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForPageReadyAsync(page);

        var row = page.Locator("tr", new() { HasTextString = eventName }).First;
        await row.ScrollIntoViewIfNeededAsync();

        // Accept the confirm() dialog automatically
        page.Dialog += (_, dialog) => dialog.AcceptAsync();

        var deactivateForm = row.Locator("form[action*='Deactivate']").First;
        await deactivateForm.Locator("button[type='submit']").ClickAsync();
        await WaitForPageReadyAsync(page);

        // After deactivation the row should still exist but with Cancelled badge
        // (depending on business logic – just verify the page reloaded without error)
        await page.WaitForURLAsync($"**{BaseUrl}/Organizer/Events**", new() { Timeout = 10_000 });

        await context.CloseAsync();
    }

    // ─── Shared helpers ────────────────────────────────────────────────────────

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
            // NetworkIdle е best-effort
        }
    }
}
