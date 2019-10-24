using System.Dynamic;

namespace System.Collections.Generic
{
    public static class IDictionary
    {
        /// <summary>
        /// Converts an IDictionary to an anonymouse object. Used in conjunction with the ToDictionary method,
        /// this pair of methods can be used to mutate an anonymous object and add a new property dynamically
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static object ToAnonymous(this IDictionary<string, object> dict)
        {
            ExpandoObject obj = new ExpandoObject();
            var coll = (ICollection<KeyValuePair<string, object>>)obj;
            foreach (var keyValPair in dict)
                coll.Add(keyValPair);
            return (object)obj;
        }
    }
}
