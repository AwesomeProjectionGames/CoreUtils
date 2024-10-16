using System;

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
    }
}