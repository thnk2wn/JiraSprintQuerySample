using System;

namespace JIRASprintQuery.Extensions.System_
{
    public static class StringExtensions
    {
        public static string Left(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            maxLength = Math.Abs(maxLength);
            return (value.Length + 3 <= maxLength ? value : value.Substring(0, maxLength -3) + "...");
        }
    }
}
