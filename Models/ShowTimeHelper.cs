using System.Globalization;

namespace CinemaWeb.Models
{
    public static class ShowTimeHelper
    {
        public static bool TryParse(string? value, out DateTime dateTime)
        {
            dateTime = default;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var candidates = new[]
            {
                value.Trim(),
                value.Contains('-') ? value.Split('-').LastOrDefault()?.Trim() ?? value.Trim() : value.Trim()
            };

            foreach (var candidate in candidates.Distinct())
            {
                if (string.IsNullOrWhiteSpace(candidate))
                    continue;

                if (DateTime.TryParse(candidate, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                    return true;

                if (DateTime.TryParseExact(candidate, "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                    return true;

                if (DateTime.TryParseExact(candidate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                    return true;

                if (DateTime.TryParseExact(candidate, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                    return true;
            }

            return false;
        }
    }
}
