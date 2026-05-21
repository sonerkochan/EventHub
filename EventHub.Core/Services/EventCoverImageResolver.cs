namespace EventHub.Core.Services
{
    public static class EventCoverImageResolver
    {
        public static string? BuildDisplayUrl(Guid? coverPhotoId, string? coverImageUrl)
        {
            if (coverPhotoId.HasValue)
            {
                return $"/photos/{coverPhotoId.Value}";
            }

            return string.IsNullOrWhiteSpace(coverImageUrl)
                ? null
                : coverImageUrl.Trim();
        }

        public static bool IsValidExternalUrl(string? coverImageUrl)
        {
            if (string.IsNullOrWhiteSpace(coverImageUrl))
            {
                return true;
            }

            return Uri.TryCreate(coverImageUrl.Trim(), UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
