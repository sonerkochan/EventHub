using EventHub.Core.Services;
using EventHub.Infrastructure.Data.Models;

namespace EventHub.Tests
{
    public class EventTranslationDisplayResolverTests
    {
        [Fact]
        public void Resolve_ReturnsBaseContent_ForEnglishCulture()
        {
            var translations = new[]
            {
                new EventTranslation
                {
                    Culture = "bg",
                    EventName = "Българско събитие",
                    Description = "Българско описание"
                }
            };

            var result = EventTranslationDisplayResolver.Resolve(
                "English Event",
                "English description",
                "en",
                translations);

            Assert.Equal("English Event", result.EventName);
            Assert.Equal("English description", result.Description);
        }

        [Fact]
        public void Resolve_ReturnsTranslation_ForBulgarianCulture()
        {
            var translations = new[]
            {
                new EventTranslation
                {
                    Culture = "bg",
                    EventName = "Българско събитие",
                    Description = "Българско описание"
                }
            };

            var result = EventTranslationDisplayResolver.Resolve(
                "English Event",
                "English description",
                "bg",
                translations);

            Assert.Equal("Българско събитие", result.EventName);
            Assert.Equal("Българско описание", result.Description);
        }

        [Fact]
        public void Resolve_FallsBackToBaseContent_WhenBulgarianTranslationIsMissing()
        {
            var result = EventTranslationDisplayResolver.Resolve(
                "English Event",
                "English description",
                "bg",
                []);

            Assert.Equal("English Event", result.EventName);
            Assert.Equal("English description", result.Description);
        }
    }
}
