using EventHub.Data;
using EventHub.Infrastructure.Data.Models;

namespace EventHub.Middlewares
{
    public class AnalyticsMiddleware
    {
        private readonly RequestDelegate _next;

        public AnalyticsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
        {
            if (context.Request.Method == HttpMethods.Get)
            {
                var ip = GetClientIp(context);
                var path = context.Request.Path.Value?.ToLower();
                // Lookup country, can remove and add another later on
                var country = await GetCountryFromIpAsync(ip ?? "");

                // Skip static resources
                if (!string.IsNullOrEmpty(path) &&
                    (path.EndsWith(".css") ||
                     path.EndsWith(".js") ||
                     path.EndsWith(".png") ||
                     path.EndsWith(".jpg") ||
                     path.EndsWith(".jpeg") ||
                     path.EndsWith(".gif") ||
                     path.EndsWith(".ico") ||
                     path.StartsWith("/lib") ||
                     path.StartsWith("/css") ||
                     path.StartsWith("/js") ||
                     path.StartsWith("/content")))
                {
                    await _next(context);
                    return;
                }

                var log = new AnalyticsLog
                {
                    IpAddress = ip,
                    Url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}",
                    Referrer = context.Request.Headers["Referer"].ToString(),
                    Timestamp = DateTime.UtcNow,
                    Country = ""
                };

                db.AnalyticsLogs.Add(log);
                await db.SaveChangesAsync();
            }

            await _next(context);
        }

        private string? GetClientIp(HttpContext context)
        {
            if (context.Request.Headers.ContainsKey("CF-Connecting-IP"))
                return context.Request.Headers["CF-Connecting-IP"].ToString();

            if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
                return context.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0];

            return context.Connection.RemoteIpAddress?.ToString();
        }
        private async Task<string> GetCountryFromIpAsync(string ip)
        {
            if (string.IsNullOrEmpty(ip) || ip == "127.0.0.1" || ip == "::1")
                return "Localhost";

            try
            {
                using var http = new HttpClient();
                http.Timeout = TimeSpan.FromSeconds(3);
                var response = await http.GetStringAsync($"https://ipapi.co/{ip}/country_name/");
                return string.IsNullOrWhiteSpace(response) ? "Unknown" : response.Trim();
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
