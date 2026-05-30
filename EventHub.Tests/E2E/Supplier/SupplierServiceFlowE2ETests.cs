using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace EventHub.Tests.E2E.Supplier;

[Trait("Category", "E2E")]
public class SupplierServiceFlowE2ETests
{
    [Fact(Skip = "Requires local environment")]
    public async Task SupplierCreatesService_ClientRequestsIt_SupplierApprovesEditsAndDeletesIt()
    {
        var baseUrl = GetRequiredEnv("EVENTHUB_BASE_URL");

        var clientEmail = GetRequiredEnv("EVENTHUB_CLIENT_EMAIL");
        var clientPassword = GetRequiredEnv("EVENTHUB_CLIENT_PASSWORD");

        var supplierEmail = GetRequiredEnv("EVENTHUB_SUPPLIER_EMAIL");
        var supplierPassword = GetRequiredEnv("EVENTHUB_SUPPLIER_PASSWORD");

        var unique = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var serviceName = $"TestService_{unique}";
        var editedName = $"TestServiceEdited_{unique}";

        using var playwright = await Playwright.CreateAsync();

        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = false,
            SlowMo = 150
        });

        var supplierCreateContext = await browser.NewContextAsync(new()
        {
            IgnoreHTTPSErrors = true
        });

        var supplierCreatePage = await supplierCreateContext.NewPageAsync();

        await LoginAsync(supplierCreatePage, baseUrl, supplierEmail, supplierPassword);

        await supplierCreatePage.GotoAsync($"{baseUrl}/Supplier/Services/Create", new()
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });

        await WaitForPageReadyAsync(supplierCreatePage);
        await ExpectVisibleTextAsync(supplierCreatePage, "Create a new service");

        await supplierCreatePage.Locator("input[name='Name']").FillAsync(serviceName);
        await supplierCreatePage.Locator("textarea[name='Description']").FillAsync("Created by automated E2E test");
        await supplierCreatePage.Locator("input[name='Price']").FillAsync("300");

        await ClickButtonAsync(supplierCreatePage, "Create");
        await WaitForPageReadyAsync(supplierCreatePage);

        await ExpectVisibleTextAsync(supplierCreatePage, serviceName);

        await supplierCreateContext.CloseAsync();

        var clientContext = await browser.NewContextAsync(new()
        {
            IgnoreHTTPSErrors = true
        });

        var clientPage = await clientContext.NewPageAsync();

        await LoginAsync(clientPage, baseUrl, clientEmail, clientPassword);

        await OpenClientServicesPageAsync(clientPage, baseUrl, serviceName);
        await ExpectVisibleTextAsync(clientPage, serviceName);

        var serviceTitle = clientPage.GetByText(serviceName, new()
        {
            Exact = false
        }).First;

        await serviceTitle.WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });

        var serviceCard = serviceTitle.Locator("xpath=ancestor::*[contains(@class, 'card')][1]");

        var messageBox = serviceCard.Locator("textarea").First;

        await messageBox.WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });

        await messageBox.FillAsync($"E2E request created at {DateTime.UtcNow:O}");

        var rentButton = serviceCard.Locator("button", new()
        {
            HasTextString = "Rent Service"
        }).First;

        await rentButton.WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });

        await rentButton.ClickAsync();
        await WaitForPageReadyAsync(clientPage);

        await ExpectAnyVisibleTextAsync(
            clientPage,
            "Service request sent to the supplier.",
            "Request pending",
            "Pending");

        await clientContext.CloseAsync();

        var supplierApproveContext = await browser.NewContextAsync(new()
        {
            IgnoreHTTPSErrors = true
        });

        var supplierApprovePage = await supplierApproveContext.NewPageAsync();

        await LoginAsync(supplierApprovePage, baseUrl, supplierEmail, supplierPassword);

        await supplierApprovePage.GotoAsync($"{baseUrl}/Supplier/Services/Index", new()
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });

        await WaitForPageReadyAsync(supplierApprovePage);

        await supplierApprovePage.GetByText("Requests", new()
        {
            Exact = true
        }).ClickAsync();

        await WaitForPageReadyAsync(supplierApprovePage);

        await ExpectVisibleTextAsync(supplierApprovePage, serviceName);

        var requestRow = supplierApprovePage.Locator("tr", new()
        {
            HasTextString = serviceName
        }).First;

        await requestRow.WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });

        var commentInput = requestRow.Locator("input[name='responseComment'], textarea[name='responseComment']").First;

        if (await commentInput.CountAsync() > 0)
        {
            await commentInput.FillAsync("Accepted by automated E2E test");
        }

        await requestRow.GetByRole(AriaRole.Button, new()
        {
            NameRegex = new Regex("Accept|Approve", RegexOptions.IgnoreCase)
        }).First.ClickAsync();

        await WaitForPageReadyAsync(supplierApprovePage);

        await ExpectAnyVisibleTextAsync(
            supplierApprovePage,
            "Service request accepted.",
            "Accepted");

        await supplierApproveContext.CloseAsync();

        var supplierEditContext = await browser.NewContextAsync(new()
        {
            IgnoreHTTPSErrors = true
        });

        var supplierEditPage = await supplierEditContext.NewPageAsync();

        await LoginAsync(supplierEditPage, baseUrl, supplierEmail, supplierPassword);

        await supplierEditPage.GotoAsync($"{baseUrl}/Supplier/Services/Index", new()
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });

        await WaitForPageReadyAsync(supplierEditPage);
        await ExpectVisibleTextAsync(supplierEditPage, serviceName);

        var createdRow = supplierEditPage.Locator("tr", new()
        {
            HasTextString = serviceName
        }).First;

        await createdRow.GetByRole(AriaRole.Link, new()
        {
            NameRegex = new Regex("Edit", RegexOptions.IgnoreCase)
        }).First.ClickAsync();

        await WaitForPageReadyAsync(supplierEditPage);

        await ExpectVisibleTextAsync(supplierEditPage, "Edit Service");

        await supplierEditPage.Locator("input[name='Name']").FillAsync(editedName);
        await supplierEditPage.Locator("textarea[name='Description']").FillAsync("Edited by automated E2E test");
        await supplierEditPage.Locator("input[name='Price']").FillAsync("350");

        await ClickButtonAsync(supplierEditPage, "Save");
        await WaitForPageReadyAsync(supplierEditPage);

        await ExpectVisibleTextAsync(supplierEditPage, editedName);

        var editedRow = supplierEditPage.Locator("tr", new()
        {
            HasTextString = editedName
        }).First;

        await editedRow.GetByRole(AriaRole.Link, new()
        {
            NameRegex = new Regex("Delete", RegexOptions.IgnoreCase)
        }).First.ClickAsync();

        await WaitForPageReadyAsync(supplierEditPage);

        await ExpectVisibleTextAsync(supplierEditPage, "Delete Service");
        await ExpectVisibleTextAsync(supplierEditPage, editedName);

        await ClickButtonAsync(supplierEditPage, "Delete");
        await WaitForPageReadyAsync(supplierEditPage);

        Assert.Equal(0, await supplierEditPage.GetByText(editedName, new()
        {
            Exact = false
        }).CountAsync());

        await supplierEditContext.CloseAsync();
    }

    private static async Task OpenClientServicesPageAsync(IPage page, string baseUrl, string serviceSearch)
    {
        var url = $"{baseUrl}/Client/Services/Index?searchTerm={Uri.EscapeDataString(serviceSearch)}";

        var response = await page.GotoAsync(url, new()
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = 30000
        });

        var status = response?.Status ?? 0;

        if (status < 200 || status >= 400)
        {
            throw new InvalidOperationException(
                $"Client services page failed. URL: {url}, HTTP status: {status}.");
        }

        await WaitForPageReadyAsync(page);
    }

    private static async Task LoginAsync(IPage page, string baseUrl, string email, string password)
    {
        await page.GotoAsync($"{baseUrl}/Identity/Account/Login", new()
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });

        await WaitForPageReadyAsync(page);

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
        await WaitForPageReadyAsync(page);

        var bodyText = await page.Locator("body").InnerTextAsync();

        if (bodyText.Contains("Invalid login attempt", StringComparison.OrdinalIgnoreCase) ||
            bodyText.Contains("Login failed", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Login failed for {email}");
        }
    }

    private static async Task FillFirstExistingAsync(IPage page, string value, params string[] selectors)
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

        throw new InvalidOperationException($"Could not find any selector: {string.Join(", ", selectors)}");
    }

    private static async Task ClickButtonAsync(IPage page, params string[] buttonNames)
    {
        foreach (var buttonName in buttonNames)
        {
            var button = page.GetByRole(AriaRole.Button, new()
            {
                NameRegex = new Regex($"^{Regex.Escape(buttonName)}$", RegexOptions.IgnoreCase)
            });

            if (await button.CountAsync() > 0)
            {
                await button.First.ClickAsync();
                return;
            }
        }

        throw new InvalidOperationException($"Could not find button: {string.Join(" / ", buttonNames)}");
    }

    private static async Task ExpectVisibleTextAsync(IPage page, string text)
    {
        await page.GetByText(text, new()
        {
            Exact = false
        }).First.WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });
    }

    private static async Task ExpectAnyVisibleTextAsync(IPage page, params string[] possibleTexts)
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);

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

        throw new TimeoutException($"None of these texts appeared: {string.Join(" / ", possibleTexts)}");
    }

    private static async Task WaitForPageReadyAsync(IPage page)
    {
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        try
        {
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new()
            {
                Timeout = 10000
            });
        }
        catch
        {
            // Some pages keep background requests alive.
        }

        await page.Locator("body").WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });
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
}
