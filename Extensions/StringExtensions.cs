﻿using System;
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
        /// Converts the input string to camel case (first character lowercase and removes spaces).
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>The input string in camel case with spaces removed</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string ToCamelCase(this string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (string.IsNullOrWhiteSpace(input)) return "";

            // Split the string by spaces, capitalize each word, and combine them
            var words = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
    }
}