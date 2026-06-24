using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace EventHub.Localization;

public static class LocalizedSelectListExtensions
{
    public static IEnumerable<SelectListItem> LocalizedEnumSelectList<TEnum>(
        this IHtmlHelper html,
        IStringLocalizer<EnumResource> localizer)
        where TEnum : struct, Enum
        => Enum.GetValues<TEnum>()
            .Select(value => new SelectListItem
            {
                Text = localizer.LocalizeEnum(value),
                Value = Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)
            });
}
