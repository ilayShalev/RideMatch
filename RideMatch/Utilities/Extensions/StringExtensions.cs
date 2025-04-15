using System;
using System.Globalization;
using System.Text;

namespace RideMatch.Utilities.Extensions
{
    public static class StringExtensions
    {
        // Checks if a string is null or empty or whitespace
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        // Gets a substring or returns empty if out of bounds
        public static string SafeSubstring(this string value, int startIndex, int length)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (startIndex < 0 || startIndex >= value.Length)
                return string.Empty;

            int availableLength = value.Length - startIndex;
            int actualLength = Math.Min(length, availableLength);

            return value.Substring(startIndex, actualLength);
        }

        // Truncates a string to a maximum length
        public static string Truncate(this string value, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Length <= maxLength)
                return value;

            int truncateLength = maxLength - suffix.Length;
            if (truncateLength <= 0)
                return suffix.SafeSubstring(0, maxLength);

            return value.SafeSubstring(0, truncateLength) + suffix;
        }

        // Converts a string to title case
        public static string ToTitleCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }
    }
}