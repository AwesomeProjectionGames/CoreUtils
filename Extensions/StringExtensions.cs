using System;
using System.Globalization;

namespace AwesomeProjectionCoreUtils.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Return the input string with the first character in upper case
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>The input string with the first character in upper case</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string FirstCharToUpper(this string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": return "";
                default: return input[0].ToString().ToUpper() + input.Substring(1);
            }
        }
        
        /// <summary>
        /// Converts the input string to camel case (first character lowercase and removes specified delimiters).
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="delimiters">The delimiters used to split the input string. Default is a space (' ').</param>
        /// <returns>The input string in camel case with specified delimiters removed</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string ToCamelCase(this string input, char[] delimiters = null)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (string.IsNullOrWhiteSpace(input)) return "";

            // Use default delimiters if none are provided
            if (delimiters == null || delimiters.Length == 0)
            {
                delimiters = new[] { ' ' };
            }

            // Split the string by the specified delimiters, capitalize each word, and combine them
            var words = input.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = words[i].FirstCharToUpper();
            }

            // Join words and ensure the first character is lowercase
            var camelCase = string.Join("", words);
            return camelCase.Length > 0
                ? camelCase[0].ToString().ToLower(CultureInfo.InvariantCulture) + camelCase.Substring(1)
                : camelCase;
        }

        /// <summary>
        /// Computes the canonical 32-bit FNV-1a hash of the UTF-8 encoded string.
        /// Adheres strictly to the 32-bit FNV-1a specification:
        /// https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// Consistent across architectures, platforms, and other programming languages (Go, Rust, Python, etc.).
        /// Zero allocations for strings up to 256 UTF-8 bytes via stackalloc Span.
        /// </summary>
        public static uint GetDeterministicHashCode(this string input)
        {
            if (input == null) return 0;

            unchecked
            {
                uint hash = 2166136261;
                const uint prime = 16777619;

                int byteCount = System.Text.Encoding.UTF8.GetByteCount(input);
                System.Span<byte> buffer = byteCount <= 256 ? stackalloc byte[byteCount] : new byte[byteCount];
                System.Text.Encoding.UTF8.GetBytes(input, buffer);

                for (int i = 0; i < buffer.Length; i++)
                {
                    hash ^= buffer[i];
                    hash *= prime;
                }

                return hash;
            }
        }
    }
}