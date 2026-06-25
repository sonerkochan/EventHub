using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace EventHub.Tests.E2E;

internal static class AdminEventPublishHelper
{
    internal static async Task PublishEventAndWaitAsync(IPage page, string eventName)
    {
        var eventRow = page.Locator("tr", new() { HasTextString = eventName }).First;
        await eventRow.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15_000 });
        await eventRow.ScrollIntoViewIfNeededAsync();

        var publishButton = eventRow.GetByRole(AriaRole.Button, new() { Name = "Publish" });
        if (await publishButton.CountAsync() == 0)
        {
            await WaitForPublishedBadgeAsync(page, eventName);
            return;
        }

        page.Dialog += async (_, dialog) => await dialog.AcceptAsync();

        var publishResponseTask = page.WaitForResponseAsync(
            response =>
                response.Url.Contains("/Admin/Events/Publish", StringComparison.OrdinalIgnoreCase) &&
                response.Request.Method == "POST",
            new() { Timeout = 15_000 });

        await publishButton.First.ClickAsync();

        var publishResponse = await publishResponseTask;
        Assert.True(
            publishResponse.Ok,
            $"Expected publish response to be successful, got {(int)publishResponse.Status} {publishResponse.StatusText}.");

        await WaitForPageReadyAsync(page);
        await WaitForPublishedBadgeAsync(page, eventName);
    }

    private static async Task WaitForPublishedBadgeAsync(IPage page, string eventName)
    {
        var updatedRow = page.Locator("tr", new() { HasTextString = eventName }).First;
        var badge = updatedRow.Locator(".badge").First;

        await Assertions.Expect(badge).ToContainTextAsync(
            new Regex("Published|Active"),
            new() { Timeout = 15_000 });
    }

    private static async Task WaitForPageReadyAsync(IPage page)
    {
        try
        {
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new() { Timeout = 15_000 });
        }
        catch
        {
        }

        try
        {
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 5_000 });
        }
        catch
        {
        }
    }
}
