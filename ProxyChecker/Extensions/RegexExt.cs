using System.Text.RegularExpressions;

namespace ProxyChecker.Extensions
{
    public static class RegexExt
    {

        /*
         * Returns the first match of a regex result
         */
        
        public static Match MatchFirst(this Regex regex, string input)
        {
            MatchCollection result = regex.Matches(input);
            return result[0];
        }
        
        
        /*
         * Checks if a regex pattern is valid with the input data
         */
        
        public static bool IsValidMatch(this Regex regex, string input)
        {
            MatchCollection result = regex.Matches(input);
            return result.Count > 0;
        }

    }
}