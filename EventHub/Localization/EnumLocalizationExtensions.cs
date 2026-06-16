using Microsoft.Extensions.Localization;

namespace EventHub.Localization;

public static class EnumLocalizationExtensions
{
    public static string LocalizeEnum<TEnum>(this IStringLocalizer<EnumResource> localizer, TEnum value)
        where TEnum : struct, Enum
    {
        var key = $"Enum.{typeof(TEnum).Name}.{value}";
        var localized = localizer[key];
        return localized.ResourceNotFound ? value.ToString() : localized.Value;
    }
}
