using System.Net;
using EventHub.Core.Models.Travelis;
using EventHub.Core.Services;
using Microsoft.Extensions.Options;

namespace EventHub.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class TravelisHotelServiceTests
{
    [Fact]
    public async Task GetHotelsByCityAsync_MapsHotelsAndBuildsImageUrls()
    {
        var city = $"Sofia-{Guid.NewGuid():N}";
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""
                [
                  {
                    "id": "hotel-1",
                    "name": "Grand Hotel Sofia",
                    "country": "Bulgaria",
                    "city": "Sofia",
                    "street": "1 Gurko Str.",
                    "postalCode": "1000",
                    "phoneNumber": "+359 2 811 0811",
                    "email": "reservations@example.com",
                    "status": 1,
                    "approved": true,
                    "images": [
                      { "id": "image-1", "name": "bg hotel 05.jpg" },
                      { "id": "image-2", "name": "room.webp" }
                    ]
                  }
                ]
                """)
        });
        var service = CreateService(handler);

        var result = await service.GetHotelsByCityAsync(city);

        var hotel = Assert.Single(result);
        Assert.Equal("Grand Hotel Sofia", hotel.Name);
        Assert.Equal("1 Gurko Str., 1000, Sofia, Bulgaria", hotel.DisplayAddress);
        Assert.Equal("+359 2 811 0811", hotel.PhoneNumber);
        Assert.Equal("reservations@example.com", hotel.Email);
        Assert.Equal("https://travelis.sadulov.com/uploads/hotels/bg%20hotel%2005.jpg", hotel.Images[0].Url);
        Assert.Equal("https://travelis.sadulov.com/uploads/hotels/room.webp", hotel.Images[1].Url);
    }

    [Fact]
    public async Task GetHotelsByCityAsync_UrlEncodesCityPathSegment()
    {
        var city = $"New York {Guid.NewGuid():N}";
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]")
        });
        var service = CreateService(handler);

        await service.GetHotelsByCityAsync(city);

        Assert.NotNull(handler.LastRequestUri);
        Assert.Contains(Uri.EscapeDataString(city), handler.LastRequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task GetHotelsByCityAsync_FiltersInactiveAndUnapprovedHotels()
    {
        var city = $"Plovdiv-{Guid.NewGuid():N}";
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""
                [
                  { "id": "active", "name": "Active Hotel", "status": 1, "approved": true },
                  { "id": "inactive", "name": "Inactive Hotel", "status": 2, "approved": true },
                  { "id": "unapproved", "name": "Unapproved Hotel", "status": 1, "approved": false }
                ]
                """)
        });
        var service = CreateService(handler);

        var result = await service.GetHotelsByCityAsync(city);

        var hotel = Assert.Single(result);
        Assert.Equal("Active Hotel", hotel.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetHotelsByCityAsync_ReturnsEmptyForBlankCity(string city)
    {
        var handler = new StubHttpMessageHandler(_ => throw new InvalidOperationException("Should not call API."));
        var service = CreateService(handler);

        var result = await service.GetHotelsByCityAsync(city);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetHotelsByCityAsync_ReturnsEmptyForNonSuccessResponse()
    {
        var city = $"Varna-{Guid.NewGuid():N}";
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = CreateService(handler);

        var result = await service.GetHotelsByCityAsync(city);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetHotelsByCityAsync_ReturnsEmptyForInvalidJson()
    {
        var city = $"Burgas-{Guid.NewGuid():N}";
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{ not-json")
        });
        var service = CreateService(handler);

        var result = await service.GetHotelsByCityAsync(city);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetHotelsByCityAsync_CachesByNormalizedCity()
    {
        var city = $"Ruse-{Guid.NewGuid():N}";
        var calls = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            calls++;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""[{ "id": "hotel-1", "name": "Cached Hotel", "status": 1, "approved": true }]""")
            };
        });
        var service = CreateService(handler);

        var first = await service.GetHotelsByCityAsync($"  {city}  ");
        var second = await service.GetHotelsByCityAsync(city);

        Assert.Single(first);
        Assert.Single(second);
        Assert.Equal(1, calls);
    }

    private static TravelisHotelService CreateService(StubHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new TravelisOptions());

        return new TravelisHotelService(httpClient, options);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> responseFactory;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> _responseFactory)
        {
            responseFactory = _responseFactory;
        }

        public Uri? LastRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            return Task.FromResult(responseFactory(request));
        }
    }
}
