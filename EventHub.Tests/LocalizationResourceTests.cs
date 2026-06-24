using System.Xml.Linq;
using EventHub.Infrastructure.Data.Models;

namespace EventHub.Tests;

public class LocalizationResourceTests
{
    [Fact]
    public void EnglishAndBulgarianResourceFilesHaveMatchingKeys()
    {
        var resourcesPath = Path.Combine(FindSolutionRoot(), "EventHub", "Resources", "Localization");
        var englishFiles = Directory.GetFiles(resourcesPath, "*.en.resx");

        Assert.NotEmpty(englishFiles);

        foreach (var englishFile in englishFiles)
        {
            var bulgarianFile = englishFile.Replace(".en.resx", ".bg.resx", StringComparison.OrdinalIgnoreCase);
            Assert.True(File.Exists(bulgarianFile), $"Missing Bulgarian resource file for {Path.GetFileName(englishFile)}.");

            var englishKeys = ReadResourceKeys(englishFile);
            var bulgarianKeys = ReadResourceKeys(bulgarianFile);

            Assert.Empty(englishKeys.Except(bulgarianKeys).OrderBy(key => key));
            Assert.Empty(bulgarianKeys.Except(englishKeys).OrderBy(key => key));
        }
    }

    [Fact]
    public void EnumResourceContainsKeysForDisplayedEnums()
    {
        var keys = ReadDomainResourceKeys("EnumResource.en.resx");
        var enumTypes = new[]
        {
            typeof(EventPriority),
            typeof(EventStatus),
            typeof(EventType),
            typeof(RoomType),
            typeof(ZoneType),
            typeof(TicketStatus),
            typeof(Payment.PaymentMethod),
            typeof(Payment.PaymentStatus),
            typeof(ApplicationType),
            typeof(ApplicationStatus),
            typeof(ServiceRentalRequestStatus)
        };

        foreach (var enumType in enumTypes)
        {
            foreach (var value in Enum.GetNames(enumType))
            {
                Assert.Contains($"Enum.{enumType.Name}.{value}", keys);
            }
        }
    }

    [Fact]
    public void ValidationResourceContainsMovedDataAnnotationKeys()
    {
        var keys = ReadDomainResourceKeys("ValidationResource.en.resx");

        var expectedKeys = new[]
        {
            "Validation.Event.TotalTickets.Range",
            "Validation.Event.Price.Range",
            "Validation.Event.Latitude.Range",
            "Validation.Event.Longitude.Range",
            "Validation.Ticket.Quantity.Range",
            "Validation.Refund.Amount.Range",
            "Validation.Passwords.DoNotMatch",
            "Validation.User.Username.Required",
            "Validation.User.Email.Required",
            "Validation.User.Password.Required",
            "Validation.User.ConfirmPassword.Required"
        };

        foreach (var key in expectedKeys)
        {
            Assert.Contains(key, keys);
        }
    }

    [Fact]
    public void PublicHomeResourceContainsHeroAndEventCardKeys()
    {
        var keys = ReadDomainResourceKeys("PublicResource.en.resx");

        var expectedKeys = new[]
        {
            "Public.Home.Hero.BeforeAccent",
            "Public.Home.Hero.Accent",
            "Public.Home.Hero.AfterAccent",
            "Public.Home.Hero.Subtitle",
            "Public.Home.BrowseEvents",
            "Public.Home.ApplyOrganizer",
            "Public.Home.UpcomingEvents",
            "Public.Home.NoEvents",
            "Public.Home.GetStartedFree"
        };

        foreach (var key in expectedKeys)
        {
            Assert.Contains(key, keys);
        }
    }

    private static string FindSolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "EventHub.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate EventHub.slnx.");
    }

    private static HashSet<string> ReadResourceKeys(string path)
        => XDocument.Load(path)
            .Root!
            .Elements("data")
            .Select(element => element.Attribute("name")?.Value)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToHashSet(StringComparer.Ordinal);

    private static HashSet<string> ReadDomainResourceKeys(string fileName)
    {
        var resourcesPath = Path.Combine(FindSolutionRoot(), "EventHub", "Resources", "Localization");
        return ReadResourceKeys(Path.Combine(resourcesPath, fileName));
    }
}
