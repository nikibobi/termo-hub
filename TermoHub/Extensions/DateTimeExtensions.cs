using System;

namespace TermoHub.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToUtcString(this DateTime date)
        {
            return date.ToString("yyyy-MM-ddTHH:mm:ss");
        }
    }
}
