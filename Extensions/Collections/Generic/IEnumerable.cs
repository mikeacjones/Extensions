using System.Data;
using System.Linq;
using System.Text;

namespace System.Collections.Generic
{
    public static class IEnumerable
    {
        /// <summary>
        /// Returns a set containing all possible combinations of the set
        /// Note: Includes the empty set combination
        /// </summary>
        /// <typeparam name="T">Type of object in set - does not matter for our purposes</typeparam>
        /// <param name="list">Set to get all sets of</param>
        /// <returns>Set of all set combinations</returns>
        public static IEnumerable<IEnumerable<T>> SetOfAllSets<T>(this IEnumerable<T> list)
        {
            T[] data = list.ToArray();
            /*
            eg, list size of 4:
            0000000000000000
            0000000000000001
            0000000000000010
            0000000000000011
            0000000000000100
            0000000000000101
            0000000000000110
            0000000000000111
            0000000000001000
            and so on...
            the bits show us our combinations, once we shift it!
            */
            return Enumerable
                    .Range(0, 1 << (data.Length)) // 2 ^ length = combinations, so create a size list: 0, 1, 2, 3, etc..
                    .Select(index => data
                        .Where((val, jIndex) => (index & (1 << jIndex)) != 0) //if the current jindex ^ 2 bits line up with our current index, include
                            .ToArray())
                    .OrderBy(@set => @set.Count()); //and then go ahead and sort them by the size of the set
        }
        /// <summary>
        /// Returns a datatable created from the object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="publicOnly">Specifies if only public properties should be added to the data table</param>
        /// <returns></returns>
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
        /// <returns></returns>
        public static string ToCSV<T>(this IEnumerable<T> items, bool publicOnly = false, bool includeHeaders = true) where T : class
        {
            StringBuilder sb = new StringBuilder();
            var props = typeof(T).GetProperties().Where(p => p.CanRead && (!publicOnly || p.GetMethod.IsPublic));

            if (includeHeaders)
                sb.AppendLine(string.Join(",", props.Select(p => p.Name))); //add the header line

            foreach (T item in items)
                sb.AppendLine(string.Join(",", props.Select(p => item.GetPropVal(p.Name))));

            return sb.ToString();
        }
    }
}
