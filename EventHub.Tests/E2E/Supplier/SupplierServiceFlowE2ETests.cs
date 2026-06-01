using Microsoft.Playwright;

namespace EventHub.Tests.E2E.Supplier;

[Trait("Category", "E2E")]
public class SupplierServiceFlowE2ETests
{
    private const string BaseUrl = "https://staging-eventhub.tryasp.net";

    private const string SupplierEmail = "supplier@test.com";
    private const string SupplierPassword = "Supplier123!";

    private const string ClientEmail = "client@test.com";
    private const string ClientPassword = "Client123!";

    [Fact]
    public async Task SupplierCreatesService_ClientRentsIt_SupplierAcceptsEditsAndDeletesIt()
    {
        var unique = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var serviceName = $"TestService_{unique}";
        var editedServiceName = $"TestServiceEdited_{unique}";

        using var playwright = await Playwright.CreateAsync();

        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = false
        });

        // SUPPLIER CREATES SERVICE

        var supplierContext = await CreateContextAsync(browser);
        var supplierPage = await supplierContext.NewPageAsync();

        await LoginAsync(
            supplierPage,
            SupplierEmail,
            SupplierPassword);

        await CreateSupplierServiceAsync(
            supplierPage,
            serviceName,
            "Created by automated E2E test",
            "300");

        await ExpectVisibleTextAsync(
            supplierPage,
            serviceName);

        await supplierContext.CloseAsync();

        // CLIENT RENTS SERVICE

        var clientContext = await CreateContextAsync(browser);
        var clientPage = await clientContext.NewPageAsync();

        await LoginAsync(
            clientPage,
            ClientEmail,
            ClientPassword);

        await ClientRentsServiceAsync(
            clientPage,
            serviceName,
            $"E2E request for {serviceName}");

        await clientContext.CloseAsync();

        // SUPPLIER ACCEPTS REQUEST, EDITS SERVICE AND DELETES IT

        var supplierFinalContext = await CreateContextAsync(browser);
        var supplierFinalPage = await supplierFinalContext.NewPageAsync();

        await LoginAsync(
            supplierFinalPage,
            SupplierEmail,
            SupplierPassword);

        await SupplierAcceptsRequestAsync(
            supplierFinalPage,
            serviceName,
            "Accepted by automated E2E test");

        await SupplierEditsAndDeletesServiceAsync(
            supplierFinalPage,
            serviceName,
            editedServiceName,
            "Edited by automated E2E test",
            "350");

        await supplierFinalContext.CloseAsync();
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

    private static async Task LoginAsync(
        IPage page,
        string username,
        string password)
    {
        await page.GotoAsync(
            $"{BaseUrl}/User/Login",
            new()
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30000
            });

        await WaitForPageReadyAsync(page);

        await page.Locator(
                "input[name='Username'], input[name='UserName'], #Username, #UserName, input[type='text']")
            .First
            .FillAsync(username);

        await page.Locator(
                "input[name='Password'], #Password, input[type='password']")
            .First
            .FillAsync(password);

        await page.GetByRole(AriaRole.Button, new()
            {
                Name = "Log in"
            })
            .ClickAsync();

        await WaitForPageReadyAsync(page);
    }

    private static async Task CreateSupplierServiceAsync(
        IPage page,
        string serviceName,
        string description,
        string price)
    {
        await page.GotoAsync(
            $"{BaseUrl}/Supplier/Services/Index",
            new()
            {
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

        await WaitForPageReadyAsync(page);

        await page
            .Locator("[data-target='#createServiceModal']")
            .First
            .ClickAsync();

        var modal = page.Locator("#createServiceModal");

        await modal.WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });

        await modal.Locator("#createName")
            .FillAsync(serviceName);

        await modal.Locator("#createDescription")
            .FillAsync(description);

        await modal.Locator("#createPrice")
            .FillAsync(price);

        await modal
            .Locator("form[action='/Supplier/Services/Create'] button[type='submit']")
            .ClickAsync();

        await WaitForPageReadyAsync(page);
    }

    private static async Task ClientRentsServiceAsync(
        IPage page,
        string serviceName,
        string message)
    {
        await page.GotoAsync(
            $"{BaseUrl}/Client/Services/Index",
            new()
            {
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

        await WaitForPageReadyAsync(page);

        await page.Locator("#SearchTerm, input[name='SearchTerm']")
            .First
            .FillAsync(serviceName);

        await page.Locator("button[type='submit'].eh-btn-search, button:has-text('Search')")
            .First
            .ClickAsync();

        await WaitForPageReadyAsync(page);

        await ExpectVisibleTextAsync(page, serviceName);

        var serviceCard = page.Locator(".eh-service-card", new()
        {
            HasTextString = serviceName
        }).First;

        await serviceCard.ScrollIntoViewIfNeededAsync();

        await serviceCard.Locator("textarea[name='message']")
            .First
            .FillAsync(message);

        await serviceCard.Locator("form[action='/Client/Services/Rent'] button[type='submit']")
            .First
            .ClickAsync();

        await WaitForPageReadyAsync(page);
    }

    private static async Task SupplierAcceptsRequestAsync(
        IPage page,
        string serviceName,
        string responseComment)
    {
        await page.GotoAsync(
            $"{BaseUrl}/Supplier/Requests/Index",
            new()
            {
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

        await WaitForPageReadyAsync(page);

        await ExpectVisibleTextAsync(page, serviceName);

        var requestRow = page.Locator("tr", new()
        {
            HasTextString = serviceName
        }).First;

        await requestRow.ScrollIntoViewIfNeededAsync();

        await requestRow
            .Locator(".item-action.dropdown a[data-toggle='dropdown'], .item-action.dropdown .icon")
            .First
            .ClickAsync();

        var acceptForm = requestRow
            .Locator("form[action='/Supplier/Requests/Accept']")
            .First;

        await acceptForm.Locator("input[name='responseComment']")
            .First
            .FillAsync(responseComment);

        await acceptForm.Locator("button[type='submit']")
            .First
            .ClickAsync();

        await WaitForPageReadyAsync(page);

        await ExpectVisibleTextAsync(page, "Accepted");
    }

    private static async Task SupplierEditsAndDeletesServiceAsync(
        IPage page,
        string originalServiceName,
        string editedServiceName,
        string editedDescription,
        string editedPrice)
    {
        await page.GotoAsync(
            $"{BaseUrl}/Supplier/Services/Index",
            new()
            {
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

        await WaitForPageReadyAsync(page);

        await ExpectVisibleTextAsync(page, originalServiceName);

        var serviceCard = page.Locator(".card", new()
        {
            HasTextString = originalServiceName
        }).First;

        await serviceCard.ScrollIntoViewIfNeededAsync();

        await serviceCard.Locator(".btn-edit-modern, a:has-text('Edit'), button:has-text('Edit')")
            .First
            .ClickAsync();

        var editModal = page.Locator(".modal.show", new()
        {
            HasTextString = "Edit Service"
        }).First;

        await editModal.WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });

        await editModal.Locator("form[action='/Supplier/Services/Edit'] input[name='Name']")
            .First
            .FillAsync(editedServiceName);

        await editModal.Locator("form[action='/Supplier/Services/Edit'] input[name='Price']")
            .First
            .FillAsync(editedPrice);

        await editModal.Locator("form[action='/Supplier/Services/Edit'] textarea[name='Description']")
            .First
            .FillAsync(editedDescription);

        await editModal.Locator("form[action='/Supplier/Services/Edit'] button[type='submit']")
            .First
            .ClickAsync();

        await WaitForPageReadyAsync(page);

        await ExpectVisibleTextAsync(page, editedServiceName);

        var editedCard = page.Locator(".card", new()
        {
            HasTextString = editedServiceName
        }).First;

        await editedCard.ScrollIntoViewIfNeededAsync();

        await editedCard.Locator(".btn-delete-modern, button:has-text('Delete')")
            .First
            .ClickAsync();

        var deleteModal = page.Locator(".modal.show", new()
        {
            HasTextString = "Confirm Delete"
        }).First;

        await deleteModal.WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });

        await deleteModal.Locator("form[action='/Supplier/Services/Delete'] button[type='submit']")
            .First
            .ClickAsync();

        await WaitForPageReadyAsync(page);

        await ExpectTextNotVisibleAsync(page, editedServiceName);
    }

    private static async Task ExpectVisibleTextAsync(
        IPage page,
        string text)
    {
        await page.GetByText(text)
            .First
            .WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
    }

    private static async Task ExpectTextNotVisibleAsync(
        IPage page,
        string text)
    {
        await page.GetByText(text)
            .First
            .WaitForAsync(new()
            {
                State = WaitForSelectorState.Detached,
                Timeout = 15000
            });
    }

    private static async Task WaitForPageReadyAsync(
        IPage page)
    {
        await page.WaitForLoadStateAsync(
            LoadState.DOMContentLoaded);

        try
        {
            await page.WaitForLoadStateAsync(
                LoadState.NetworkIdle,
                new()
                {
                    Timeout = 10000
                });
        }
        catch
        {
        }
    }
}
