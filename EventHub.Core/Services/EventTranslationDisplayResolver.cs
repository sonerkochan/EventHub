using EventHub.Infrastructure.Data.Models;

namespace EventHub.Core.Services
{
    public static class EventTranslationDisplayResolver
    {
        public static (string EventName, string? Description) Resolve(
            string eventName,
            string? description,
            string culture,
            IEnumerable<EventTranslation> translations)
        {
            if (string.Equals(culture, "en", StringComparison.OrdinalIgnoreCase))
            {
                return (eventName, description);
            }

            var translation = translations.FirstOrDefault(t =>
                string.Equals(t.Culture, culture, StringComparison.OrdinalIgnoreCase));

            return translation == null
                ? (eventName, description)
                : (translation.EventName, translation.Description ?? description);
        }
    }
}
