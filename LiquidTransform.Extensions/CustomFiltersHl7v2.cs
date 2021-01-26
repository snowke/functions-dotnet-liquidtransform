using DotLiquid;
using System.Text.RegularExpressions;

namespace LiquidTransform.Extensions
{
    public static class CustomFiltersHl7v2
    {
        static readonly Regex regexReplaceTrailingDelimitersExcludingPipes = new Regex(@"(\s|\^|\~|\&)+(\^|\~|\&|\||$)", RegexOptions.Compiled | RegexOptions.Multiline);
        static readonly Regex regexReplaceTrailingPipes = new Regex(@"(\|)+(\||$)", RegexOptions.Compiled | RegexOptions.Multiline);
        static readonly Regex regexReplaceSegmentDelimiters = new Regex(@"(\r|\n){2,}", RegexOptions.Compiled | RegexOptions.Multiline);

        /// <summary>
        ///     Escape reserved HL7 v2 characters.  For example, | -> \F\
        ///     The special character escape sequences (\F\, \S\, \R\, \T\, \P\ and \E\) 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <returns>string with characters escaped based on HL7v2 rules</returns>
        public static string Hl7v2_Escape(Context context, string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input.Replace("\\", "\\E\\") // Escape delimiter - must be first replacement
                        .Replace("|", "\\F\\")  // Field delimiter
                        .Replace("~", "\\R\\")  // Repeat delimiter
                        .Replace("^", "\\S\\")  // Component delimiter
                        .Replace("&", "\\T\\")  // Sub-Component delimiter
                        .Replace("#", "\\P\\")  // Truncation delimiter
                        .Trim();
        }

        /// <summary>
        ///     Unescape reserved HL7 v2 characters.  For example, \F\ -> |
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <returns>string with characters unescaped based on HL7v2 rules</returns>
        public static string Hl7v2_Unescape(Context context, string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            return input.Replace("\\F\\", "|")  // Field delimiter
                        .Replace("\\R\\", "~")  // Repeat delimiter
                        .Replace("\\S\\", "^")  // Component delimiter
                        .Replace("\\T\\", "&")  // Sub-Component delimiter
                        .Replace("\\P\\", "#")  // Truncation delimiter
                        .Replace("\\E\\", "\\");// Escape delimiter - must be last replacement
        }

        /// <summary>
        ///     Clean trailing delimeters and ensure message segments (lines) end in carriage return and not newline
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <returns>hl7 v2 message cleaned trailing delimeters and segment separators by carriage return</returns>
        public static string Hl7v2_CleanMessage(Context context, string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            input = regexReplaceTrailingDelimitersExcludingPipes.Replace(input, string.Empty);
            input = regexReplaceTrailingPipes.Replace(input, string.Empty);
            input = regexReplaceSegmentDelimiters.Replace(input, string.Empty);

            return input;
        }
    }
}
