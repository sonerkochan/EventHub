using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace EventHub.Tests.E2E.Supplier;

public class SupplierServiceFlowE2ETests
{
    [Fact]
    public async Task ClientRequestsSupplierService()
    {
        var baseUrl = GetRequiredEnv("EVENTHUB_BASE_URL");

        var requesterEmail = GetRequiredEnv("EVENTHUB_CLIENT_EMAIL");
        var requesterPassword = GetRequiredEnv("EVENTHUB_CLIENT_PASSWORD");

        var serviceSearch = GetOptionalEnv("EVENTHUB_SERVICE_SEARCH", "Sound");
        var serviceName = GetOptionalEnv("EVENTHUB_SERVICE_NAME", "Sound System");

        using var playwright = await Playwright.CreateAsync();

        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = false,
            SlowMo = 150
        });

        var context = await browser.NewContextAsync(new()
        {
            IgnoreHTTPSErrors = true
        });

        var page = await context.NewPageAsync();

        await LoginAsync(page, baseUrl, requesterEmail, requesterPassword);

        await OpenClientServicesPageAsync(page, baseUrl, serviceSearch);

        await ExpectVisibleTextAsync(page, serviceName);

        var serviceCard = page.Locator(".card", new()
        {
            HasTextString = serviceName
        }).First;

        var button = serviceCard.GetByRole(AriaRole.Button, new()
        {
            NameRegex = new Regex("Rent Service|Request Again|Request Service|Rent", RegexOptions.IgnoreCase)
        });

        if (await button.CountAsync() == 0)
        {
            await ExpectVisibleTextAsync(page, "Request pending");
            await context.CloseAsync();
            return;
        }

        await serviceCard
            .Locator("textarea[name='message']")
            .First
            .FillAsync($"E2E request created at {DateTime.UtcNow:O}");

        await button.First.ClickAsync();

        await ExpectAnyVisibleTextAsync(
            page,
            "Service request sent to the supplier.",
            "Unable to request this service. You may already have a pending request.",
            "Request pending");

        await context.CloseAsync();
    }

    [Fact]
    public async Task SupplierCreatesEditsAndDeletesService()
    {
        var baseUrl = GetRequiredEnv("EVENTHUB_BASE_URL");

        var supplierEmail = GetRequiredEnv("EVENTHUB_SUPPLIER_EMAIL");
        var supplierPassword = GetRequiredEnv("EVENTHUB_SUPPLIER_PASSWORD");

        var unique = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var serviceName = $"E2E Service {unique}";
        var editedName = $"E2E Service Edited {unique}";

        using var playwright = await Playwright.CreateAsync();

        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = false,
            SlowMo = 150
        });

        var context = await browser.NewContextAsync(new()
        {
            IgnoreHTTPSErrors = true
        });

        var page = await context.NewPageAsync();

        await LoginAsync(page, baseUrl, supplierEmail, supplierPassword);

        await page.GotoAsync($"{baseUrl}/Supplier/Services/Index", new()
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });

        await ExpectVisibleTextAsync(page, "My Services");

        await page.GotoAsync($"{baseUrl}/Supplier/Services/Create", new()
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });

        await ExpectVisibleTextAsync(page, "Create a new service");

        await page.Locator("input[name='Name']").FillAsync(serviceName);
        await page.Locator("textarea[name='Description']").FillAsync("Created by automated E2E test");
        await page.Locator("input[name='Price']").FillAsync("300");

        await ClickButtonAsync(page, "Create");

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await ExpectVisibleTextAsync(page, serviceName);

        var createdRow = page.Locator("tr", new()
        {
            HasTextString = serviceName
        }).First;

        await createdRow.GetByRole(AriaRole.Link, new()
        {
            Name = "Edit"
        }).ClickAsync();

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await ExpectVisibleTextAsync(page, "Edit Service");

        await page.Locator("input[name='Name']").FillAsync(editedName);
        await page.Locator("textarea[name='Description']").FillAsync("Edited by automated E2E test");
        await page.Locator("input[name='Price']").FillAsync("350");

        await ClickButtonAsync(page, "Save");

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await ExpectVisibleTextAsync(page, editedName);

        var editedRow = page.Locator("tr", new()
        {
            HasTextString = editedName
        }).First;

        await editedRow.GetByRole(AriaRole.Link, new()
        {
            Name = "Delete"
        }).ClickAsync();

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await ExpectVisibleTextAsync(page, "Delete Service");
        await ExpectVisibleTextAsync(page, editedName);

        await ClickButtonAsync(page, "Delete");

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var deletedService = page.GetByText(editedName, new()
        {
            Exact = false
        });

        Assert.Equal(0, await deletedService.CountAsync());

        await context.CloseAsync();
    }

    private static async Task OpenClientServicesPageAsync(
        IPage page,
        string baseUrl,
        string serviceSearch)
    {
        var url = GetOptionalEnv(
            "EVENTHUB_REQUESTER_SERVICES_URL",
            $"{baseUrl}/Client/Services/Index?searchTerm={Uri.EscapeDataString(serviceSearch)}");

        var response = await page.GotoAsync(url, new()
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = 15000
        });

        var status = response?.Status ?? 0;

        if (status < 200 || status >= 400)
        {
            throw new InvalidOperationException(
                $"Client services page failed. URL: {url}, HTTP status: {status}.");
        }
    }

    private static async Task LoginAsync(
        IPage page,
        string baseUrl,
        string email,
        string password)
    {
        await page.GotoAsync($"{baseUrl}/Identity/Account/Login", new()
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });

        await FillFirstExistingAsync(
            page,
            email,
            "input[name='Input.Email']",
            "input[name='Email']",
            "input[type='email']",
            "#Input_Email",
            "#Email");

        await FillFirstExistingAsync(
            page,
            password,
            "input[name='Input.Password']",
            "input[name='Password']",
            "input[type='password']",
            "#Input_Password",
            "#Password");

        await ClickButtonAsync(page, "Log in", "Login", "Вход");

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var bodyText = await page.Locator("body").InnerTextAsync();

        if (bodyText.Contains("Invalid login attempt", StringComparison.OrdinalIgnoreCase) ||
            bodyText.Contains("Login failed", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Login failed for {email}");
        }
    }

    private static async Task FillFirstExistingAsync(
        IPage page,
        string value,
        params string[] selectors)
    {
        foreach (var selector in selectors)
        {
            var locator = page.Locator(selector);

            if (await locator.CountAsync() > 0)
            {
                await locator.First.FillAsync(value);
                return;
            }
        }

        throw new InvalidOperationException(
            $"Could not find any selector: {string.Join(", ", selectors)}");
    }

    private static async Task ClickButtonAsync(
        IPage page,
        params string[] buttonNames)
    {
        foreach (var buttonName in buttonNames)
        {
            var button = page.GetByRole(
                AriaRole.Button,
                new()
                {
                    NameRegex = new Regex(
                        $"^{Regex.Escape(buttonName)}$",
                        RegexOptions.IgnoreCase)
                });

            if (await button.CountAsync() > 0)
            {
                await button.First.ClickAsync();
                return;
            }
        }

        throw new InvalidOperationException(
            $"Could not find button: {string.Join(" / ", buttonNames)}");
    }

    private static async Task ExpectVisibleTextAsync(IPage page, string text)
    {
        await page.GetByText(text, new()
        {
            Exact = false
        }).First.WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = 10000
        });
    }

    private static async Task ExpectAnyVisibleTextAsync(
        IPage page,
        params string[] possibleTexts)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);

        while (DateTime.UtcNow < deadline)
        {
            foreach (var text in possibleTexts)
            {
                var locator = page.GetByText(text, new()
                {
                    Exact = false
                });

                if (await locator.CountAsync() > 0)
                {
                    return;
                }
            }

            await page.WaitForTimeoutAsync(250);
        }

        throw new TimeoutException(
            $"None of these texts appeared: {string.Join(" / ", possibleTexts)}");
    }

    private static string GetRequiredEnv(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing environment variable: {key}");
        }

        return value;
    }

    private static string GetOptionalEnv(string key, string fallback)
    {
        var value = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}