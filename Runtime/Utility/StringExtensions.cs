using System;
using System.Linq; // Required for .Select()
using System.Globalization; // Required for TextInfo
using System.Text;
using System.Text.RegularExpressions;

namespace Shared.Foundation
{
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a snake_case string to PascalCase.
        /// </summary>
        /// <param name="str">The input snake_case string.</param>
        /// <returns>The PascalCase equivalent of the input string.</returns>
        public static string SnakeCaseToPascalCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            // Split the string by underscores
            string[] words = str.Split('_');

            // Capitalize the first letter of each word and concatenate them
            return string.Concat(words.Select(word =>
                CultureInfo.CurrentCulture.TextInfo.ToTitleCase(word)
            ));
        }

        public static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Thêm dấu gạch dưới trước chữ hoa (trừ ký tự đầu tiên)
            var result = Regex.Replace(input, "([a-z0-9])([A-Z])", "$1_$2");
            result = Regex.Replace(result, "([A-Z])([A-Z][a-z])", "$1_$2");

            return result.ToLowerInvariant();
        }
    }
}