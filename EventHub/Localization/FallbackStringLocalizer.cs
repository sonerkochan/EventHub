using System.Globalization;
using Microsoft.Extensions.Localization;

namespace EventHub.Localization;

public sealed class FallbackStringLocalizer<TResource> : IStringLocalizer<TResource>
{
    public LocalizedString this[string name]
        => new(name, name, resourceNotFound: true);

    public LocalizedString this[string name, params object[] arguments]
        => new(name, string.Format(CultureInfo.CurrentCulture, name, arguments), resourceNotFound: true);

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        => Enumerable.Empty<LocalizedString>();
}
