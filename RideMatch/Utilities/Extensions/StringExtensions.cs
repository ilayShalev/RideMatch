using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Utilities.Extensions
{
    public static class StringExtensions
    {
        // Checks if a string is null or empty or whitespace
        public static bool IsNullOrEmpty(this string value);

        // Gets a substring or returns empty if out of bounds
        public static string SafeSubstring(this string value, int startIndex, int length);

        // Truncates a string to a maximum length
        public static string Truncate(this string value, int maxLength, string suffix = "...");

        // Converts a string to title case
        public static string ToTitleCase(this string value);
    }
}
