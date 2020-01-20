using System.Data;
using System.Linq;
using System.Text;

namespace System.Collections.Generic
{
    /// <summary>
    /// Provides extensions for IEnumerable objects
    /// </summary>
    public static class IEnumerable
    {
        /// <summary>
        /// Returns a set containing all possible combinations of the set
        /// Note: Includes the empty set combination
        /// </summary>
        /// <typeparam name="T">Type of object in set - does not matter for our purposes</typeparam>
        /// <param name="list">Set to get all sets of</param>
        /// <returns>Set of all set combinations</returns>
        public static IEnumerable<IEnumerable<T>> SetOfAllSets<T>(this IEnumerable<T> list, bool includeEmptySet = true)
        {
            //if its an empty set return an empty set
            if (list == null || list.Count() == 0)
                if (includeEmptySet)
                    return new[] { new T[0] };
                else
                    return null;

            #region Explanation
            /*
                For set: { 1, 2, 3, 4, 5 }

                2^0=1  : 00001
                2^1=2  : 00010
                2^2=4  : 00100
                2^3=8  : 01000
                2^4=16 : 10000

                0:  00000 = no matches
                1:  00001 = first element
                2:  00010 = second element
                3:  00011 = first & second element
                4:  00100 = third element
                5:  00101 = first & third element
                6:  00110 = second and third element
                7:  00111 = first, second, and third element
                8:  01000 = fourth element
                9:  01001 = first and fourth element
                10: 01010 = second and fourth element
                11: 01011 = first, second, and fourth element
                12: 01100 = third and fourth element
                13: 01101 = first, third, and fourth element
                14: 01110 = second, third, and fourth element
                15: 01111 = first, second, third, and fourth element
                16: 10000 = fifth element
                17: 10001 = first element and fifth element
                18: 10010 = second element and fifth element
                19: 10011 = first, second, and fifth element
                20: 10100 = third and fifth element
                21: 10101 = first, third, and fifth element
                22: 10110 = second, third, and fifth element
                23: 10111 = first, second, third, and fifth element
                24: 11000 = fourth and fifth element
                25: 11001 = first, fourth, and fifth element
                26: 11010 = second, fourth and fifth element
                27: 11011 = first, second, fourth and fifth element
                28: 11100 = third, fourth and fifth element
                29: 11101 = first, third, fourth, and fifth element
                30: 11110 = second, third, fourth, and fifth element
                31: 11111 = first, second, third, fourth, and fifth element
                */
            #endregion
            int numberOfMasks = 1 << list.Count();
            if (!includeEmptySet) --numberOfMasks;

            return Enumerable
                    .Range(includeEmptySet ? 0 : 1, numberOfMasks) // 2 ^ length = combinations, so create a size list: 0, 1, 2, 3, etc..
                    .Select(index => list
                        .Where((val, jIndex) => (index & (1 << jIndex)) != 0)
                            .ToArray())
                    .OrderBy(@set => @set.Count()); //and then go ahead and sort them by the size of the set
        }
        /// <summary>
        /// Returns a datatable created from the object. If the type can be determined, the column type will be set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="publicOnly">Specifies if only public properties should be added to the data table</param>
        /// <returns>DataTable - one row per object, with a column for each property.</returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> items, bool publicOnly = false) where T : class
        {
            DataTable result = new DataTable();

            var props = typeof(T)
                .GetProperties()
                .Where(p => p.CanRead && (!publicOnly || p.GetMethod.IsPublic));

            foreach (var prop in props)
                result
                    .Columns
                    .Add(new DataColumn(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType)); //try and be type safe with our data types

            foreach (var item in items)
            {
                DataRow row = result.NewRow();
                foreach (var prop in props)
                {
                    row[prop.Name] = prop.GetValue(item, null) ?? DBNull.Value; //if the value is null, pass in DBNull.Value so a null gets passed
                }
                result.Rows.Add(row);
            }

            return result;
        }
        /// <summary>
        /// Converts an IEnumerable to a csv string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="publicOnly">Indicates if only public properties should be included</param>
        /// <param name="includeHeaders">Indicates if a header line should be included</param>
        /// <returns>A string containing the CSV representation of the object.</returns>
        public static string ToCSV<T>(this IEnumerable<T> items, string separator = ",", string wrapper = "\"", bool publicOnly = false, bool includeHeaders = true) where T : class
        {
            StringBuilder sb = new StringBuilder();
            var props = typeof(T).GetProperties().Where(p => p.CanRead && (!publicOnly || p.GetMethod.IsPublic));

            if (includeHeaders)
                sb.AppendLine(string.Join(separator, props.Select(p => p.Name))); //add the header line

            foreach (T item in items)
                sb.AppendLine(string.Join(separator, props.Select(p =>
                {
                    string propValue = item.GetPropVal(p.Name).ToString();
                    if (propValue.Contains(","))
                        propValue = $"{wrapper}{propValue.Replace(wrapper, $"\\{wrapper}")}{wrapper}";
                    return propValue;
                })));

            return sb.ToString();
        }
    }
}
