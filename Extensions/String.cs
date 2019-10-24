using System.Linq;
using System.Text.RegularExpressions;

namespace System
{
    /// <summary>
    /// Provides string extensions
    /// </summary>
    public static class String
    {
        /// <summary>
        /// Takes an object with attributes and does runtime string interpolation
        /// </summary>
        /// <param name="s"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string Interpolate(this string s, object values)
        {
            string tempString = s;
            //retrieve all the properties - these represent the keys we will look for
            var props = values
                .GetType()
                .GetProperties()
                .Where(p => p.PropertyType.IsPublic)
                .Where(p => p.CanRead);

            foreach (var prop in props)
            {
                string regexExpression = "{" + prop.Name + "((?:[^}]*))}";
                //only grab the unique matches, ie: if {ArrivalDate} is in multiple times, we only need to call replace once.
                var matches = Regex
                    .Matches(s, regexExpression)
                    .OfType<Match>()
                    .Select(m => m.Value)
                    .Distinct();
                if (matches.Count() == 0) continue; //well, nothing to do!

                var val = prop.GetValue(values);
                foreach (string match in matches)
                {
                    //this is a lot of string replacements - better way of doing this?
                    //could do a final string format, but then have to worry if the source has 1) keys with no matches values
                    //and 2) if there are any erroneous curly braces.
                    string newMatch = match.Replace(prop.Name, "0");
                    string stringValue = string.Format(newMatch, val);
                    tempString = tempString.Replace(match, stringValue);
                }
            }
            return tempString;
        }
    }
}
