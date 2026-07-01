using Microsoft.Playwright;
using EventHub.Tests.E2E;

namespace EventHub.Tests.E2E.Admin;

[Trait("Category", "E2E")]
public class AdminAreaE2ETests
{
    private const string BaseUrl = "https://staging-eventhub.tryasp.net";

    private const string AdminUsername = "testadmin";
    private const string AdminPassword = "Testadmin123!";

    [Fact]
    public async Task Admin_CanLoginAndOpenDashboard()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = false
        });

        var context = await CreateContextAsync(browser);
        var page = await context.NewPageAsync();

        await LoginAsAdminAsync(page);
        await GoToAdminAsync(page, "/Admin/Home/Index");

        await ExpectVisibleTextAsync(page, "Welcome back");
        await ExpectVisibleTextAsync(page, "Total Users");
        await ExpectVisibleTextAsync(page, "Total Events");
        await ExpectVisibleTextAsync(page, "Tickets Sold");
        await ExpectVisibleTextAsync(page, "Total Revenue");
        await ExpectVisibleTextAsync(page, "Pending Items");

        await context.CloseAsync();
    }

    [Fact]
    public async Task Admin_CanCreateVenueRoomAndEvent()
    {
        var unique = UniqueSuffix();
        var venueName = $"E2E Venue {unique}";
        var roomName = $"E2E Room {unique}";
        var eventName = $"E2E Event {unique}";

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = false
        });

        var context = await CreateContextAsync(browser);
        var page = await context.NewPageAsync();

        await LoginAsAdminAsync(page);
        await CreateVenueAsync(page, venueName);
        await CreateRoomAsync(page, roomName, venueName, capacity: 8);
        await CreateEventAsync(page, eventName, roomName, totalTickets: 8);
        await PublishEventAsync(page, eventName);

        await ExpectVisibleTextAsync(page, eventName);
        var eventRow = page.Locator("tr", new() { HasTextString = eventName }).First;
        await eventRow.GetByText("Published").First.WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });

        await context.CloseAsync();
    }

    [Fact]
    public async Task Admin_CanManageSeatLayoutAndZones()
    {
        var unique = UniqueSuffix();
        var venueName = $"E2E Layout Venue {unique}";
        var roomName = $"E2E Layout Room {unique}";
        var zoneName = $"E2E VIP {unique}";

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = false
        });

        var context = await CreateContextAsync(browser);
        var page = await context.NewPageAsync();

        await LoginAsAdminAsync(page);
        await CreateVenueAsync(page, venueName);
        await CreateRoomAsync(page, roomName, venueName, capacity: 6);
        await OpenRoomLayoutAsync(page, roomName);

        await page.Locator("#gridRows").FillAsync("2");
        await page.Locator("#gridCols").FillAsync("3");
        await page.GetByRole(AriaRole.Button, new() { Name = "Resize" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Fill All" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Save Seats" }).ClickAsync();

        await page.Locator("#dirtyBanner").WaitForAsync(new()
        {
            State = WaitForSelectorState.Hidden,
            Timeout = 15000
        });
        await page.Locator("#seatGrid .seat-cell.seat-placed:not(.seat-unsaved)").First.WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });

        await page.GetByRole(AriaRole.Button, new() { Name = "Zones Mode" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "+ Zone" }).ClickAsync();
        await page.Locator("#newZoneName").FillAsync(zoneName);
        await page.Locator("#newZoneType").SelectOptionAsync("0");
        await page.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();

        await ExpectVisibleTextAsync(page, zoneName);

        await SelectPlacedSeatAsync(page, 0, "1 selected");
        await SelectPlacedSeatAsync(page, 1, "2 selected");

        await page.GetByRole(AriaRole.Button, new() { Name = "Assign selected seats" }).ClickAsync();

        var zoneItem = page.Locator("#zoneList .zone-item", new() { HasTextString = zoneName }).First;
        await zoneItem.GetByText("2 seats").WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });

        await context.CloseAsync();
    }

    private static async Task<IBrowserContext> CreateContextAsync(IBrowser browser)
    {
        return await browser.NewContextAsync(new()
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new()
            {
                Width = 1366,
                Height = 768
            }
        });
    }

    private static async Task LoginAsAdminAsync(IPage page)
    {
        await page.GotoAsync(
            $"{BaseUrl}/User/Login",
            new()
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30000
            });

        await WaitForPageReadyAsync(page);

        await page.Locator("input[name='Username'], input[name='UserName'], #Username, #UserName, input[type='text']")
            .First
            .FillAsync(AdminUsername);

        await page.Locator("input[name='Password'], #Password, input[type='password']")
            .First
            .FillAsync(AdminPassword);

        await page.GetByRole(AriaRole.Button, new()
        {
            Name = "Log in"
        }).ClickAsync();

        await WaitForPageReadyAsync(page);
    }

    private static async Task GoToAdminAsync(IPage page, string path)
    {
        await page.GotoAsync(
            $"{BaseUrl}{path}",
            new()
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30000
            });

        await WaitForPageReadyAsync(page);
    }

    private static async Task CreateVenueAsync(IPage page, string venueName)
    {
        await GoToAdminAsync(page, "/Admin/Venues/Index");
        await page.GetByRole(AriaRole.Button, new() { Name = "+ New Venue" }).ClickAsync();

        var modal = page.Locator("#venueModal");
        await modal.WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });

        await modal.Locator("input[name='Name']").FillAsync(venueName);
        await modal.Locator("textarea[name='Description']").FillAsync("Created by Admin E2E test.");
        await modal.Locator("input[name='Address']").FillAsync("1 E2E Street");
        await modal.Locator("input[name='City']").FillAsync("Sofia");
        await modal.Locator("input[name='Country']").FillAsync("Bulgaria");
        await modal.Locator("input[name='PostalCode']").FillAsync("1000");
        await modal.Locator("input[name='Latitude']").FillAsync("42.6977");
        await modal.Locator("input[name='Longitude']").FillAsync("23.3219");
        await modal.Locator("input[name='ContactEmail']").FillAsync("admin-e2e@example.com");
        await modal.Locator("input[name='ContactPhone']").FillAsync("0888123456");

        await modal.Locator("button[type='submit']").ClickAsync();
        await WaitForPageReadyAsync(page);
        await ExpectVisibleTextAsync(page, venueName);
    }

    private static async Task CreateRoomAsync(IPage page, string roomName, string venueName, int capacity)
    {
        await GoToAdminAsync(page, "/Admin/Rooms/Index");
        await page.GetByRole(AriaRole.Button, new() { Name = "+ New Room" }).ClickAsync();

        var modal = page.Locator("#roomModal");
        await modal.WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });

        await modal.Locator("input[name='Name']").FillAsync(roomName);
        await modal.Locator("textarea[name='Description']").FillAsync("Created by Admin E2E test.");
        await modal.Locator("select[name='VenueId']").SelectOptionAsync(new[]
        {
            new SelectOptionValue
            {
                Label = $"{venueName} (Sofia)"
            }
        });
        await modal.Locator("input[name='Capacity']").FillAsync(capacity.ToString());

        await modal.Locator("button[type='submit']").ClickAsync();
        await WaitForPageReadyAsync(page);
        await ExpectVisibleTextAsync(page, roomName);
    }

    private static async Task CreateEventAsync(IPage page, string eventName, string roomName, int totalTickets)
    {
        await GoToAdminAsync(page, "/Admin/Events/Index");
        await page.GetByRole(AriaRole.Button, new() { Name = "+ New Event" }).ClickAsync();

        var modal = page.Locator("#eventModal");
        await modal.WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });

        var start = DateTime.UtcNow.AddDays(14);
        var end = start.AddHours(2);

        await modal.Locator("input[name='EventName']").FillAsync(eventName);
        await modal.Locator("textarea[name='Description']").FillAsync("Created by Admin E2E test.");
        await FillIfPresentAsync(modal, "input[name='BulgarianEventName']", $"{eventName} BG");
        await FillIfPresentAsync(modal, "textarea[name='BulgarianDescription']", "Created by Admin E2E test.");
        await modal.Locator("select[name='RoomId']").SelectOptionAsync(new[]
        {
            new SelectOptionValue
            {
                Label = $"{roomName} (cap. {totalTickets})"
            }
        });
        await modal.Locator("input[name='StartDateTime']").FillAsync(start.ToString("yyyy-MM-ddTHH:mm"));
        await modal.Locator("input[name='EndDateTime']").FillAsync(end.ToString("yyyy-MM-ddTHH:mm"));
        await modal.Locator("input[name='TotalTickets']").FillAsync(totalTickets.ToString());
        await modal.Locator("input[name='BasePrice']").FillAsync("25.00");
        await modal.Locator("input[name='Address']").FillAsync("1 E2E Event Street");
        await modal.Locator("input[name='City']").FillAsync("Sofia");
        await modal.Locator("input[name='CountryCode']").FillAsync("BG");
        await modal.Locator("input[name='Latitude']").FillAsync("42.6977");
        await modal.Locator("input[name='Longitude']").FillAsync("23.3219");

        await modal.Locator("button[type='submit']").ClickAsync();
        await WaitForPageReadyAsync(page);
        await ExpectVisibleTextAsync(page, eventName);
    }

    private static async Task PublishEventAsync(IPage page, string eventName)
    {
        await AdminEventPublishHelper.PublishEventAndWaitAsync(page, eventName);
    }

    private static async Task OpenRoomLayoutAsync(IPage page, string roomName)
    {
        await GoToAdminAsync(page, "/Admin/Rooms/Index");
        var row = page.Locator("tr", new() { HasTextString = roomName }).First;
        await row.GetByRole(AriaRole.Link, new() { Name = "Layout" }).ClickAsync();
        await WaitForPageReadyAsync(page);
        await ExpectVisibleTextAsync(page, "Layout Editor");
    }

    private static async Task SelectPlacedSeatAsync(IPage page, int index, string expectedSelectionText)
    {
        var seat = page.Locator("#seatGrid .seat-cell.seat-placed").Nth(index);
        await seat.WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });

        await seat.DispatchEventAsync("mousedown");
        await page.Locator("body").DispatchEventAsync("mouseup");

        await page.Locator("#selectionStatus").GetByText(expectedSelectionText).WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });
    }

    private static async Task FillIfPresentAsync(ILocator parent, string selector, string value)
    {
        var locator = parent.Locator(selector);
        if (await locator.CountAsync() > 0)
        {
            await locator.First.FillAsync(value);
        }
    }

    private static async Task ExpectVisibleTextAsync(IPage page, string text)
    {
        await page.GetByText(text)
            .First
            .WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
    }

    private static async Task WaitForPageReadyAsync(IPage page)
    {
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    }

    private static string UniqueSuffix()
        => DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
}
