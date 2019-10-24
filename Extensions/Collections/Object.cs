using System.Collections.Generic;
using System.Linq;

namespace System.Collections
{
    public static class Object
    {

        /// <summary>
        /// Provides a method for updating all public attributes that can be set - easy way to take a source object and update it from a target
        /// </summary>
        /// <typeparam name="T">T - object type to update, must be a  class</typeparam>
        /// <param name="target">Object that will be updated</param>
        /// <param name="source">Object to update from</param>
        public static void UpdateProps<T>(this T target, T source) where T : class
        {
            var props = typeof(T).GetProperties()
                .Where(p => p.CanRead)
                .Where(p => p.CanWrite)
                .Where(p => p.GetMethod.IsPublic)
                .Where(p => p.SetMethod.IsPublic);
            foreach (var prop in props)
            {
                prop.SetValue(target, prop.GetValue(source));
            }
        }
        /// <summary>
        /// Converts an object to an IDictionary. Used in conjunction with the ToAnonymous method,
        /// this pair of methods can be used to mutate an anonymous object and add a new property dynamically
        /// </summary>
        /// <typeparam name="T">Type to convert - must be a class</typeparam>
        /// <param name="o">Object being converterd</param>
        /// <returns>IDictionary of all public, readable attributes</returns>
        public static IDictionary<string, object> ToDictionary<T>(this T o, bool publicOnly = false) where T : class
        {
            var anonymousType = o.GetType();
            var properties = anonymousType.GetProperties().Where(p => p.CanRead && (!publicOnly || p.GetMethod.IsPublic));
            IDictionary<string, object> res = new Dictionary<string, object>();
            foreach (var prop in properties)
                res[prop.Name] = o.GetPropVal(prop.Name);

            return res;
        }
        /// <summary>
        /// Anonymous objects can't be modified - they are essentially dynamic, readonly objects created at run-time
        /// This hacky extension provides a method for changing a value on an anonymous object... in the same way that
        /// you change the value of a string. It creates and returns a new anonymous object
        /// </summary>
        /// <param name="o">Object to mutate</param>
        /// <param name="propName">Name of the property to change the value on</param>
        /// <param name="value">New value for the property</param>
        /// <returns></returns>
        public static object MutateAnonymous(this object o, string propName, object value)
        {
            var anonymousType = o.GetType();
            var properties = anonymousType.GetProperties();
            var propertyTypes = properties.Select(p => p.PropertyType).ToArray();

            //The constructor has parameters for each property on the type
            var constructor = anonymousType.GetConstructor(propertyTypes);

            //clone the existing values to pass to ConstructorInfo
            var values = properties.Select(p => {
                return p.Name == propName ? value : p.GetValue(o);
            }).ToArray();
            var anonymousClone = constructor.Invoke(values);

            return anonymousClone;
        }
        /// <summary>
        /// Checks if the object contains the property - useful for anonymous objects
        /// </summary>
        /// <param name="o"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static bool ContainsProperty<T>(this T o, string propName) where T : class
        {
            var prop = o.GetType().GetProperty(propName);
            return prop != null;
        }
        /// <summary>
        /// Sets the value of the specified property
        /// </summary>
        /// <param name="o"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetPropVal<T>(this T o, string propName, object value) where T : class
        {
            var prop = o.GetType().GetProperty(propName);
            if (prop == null)
                throw new InvalidOperationException("Property does not eixst on object");
            prop.SetValue(o, value);
        }
        /// <summary>
        /// Returns the value of the specified property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static object GetPropVal<T>(this T o, string propName) where T : class
        {
            var prop = o.GetType().GetProperty(propName);
            if (prop == null)
                throw new InvalidOperationException("Property does not exist on object");
            return prop.GetValue(o);
        }
        /// <summary>
        /// Get the string value of an objects specified property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static string GetStringPropVal<T>(this T o, string propName) where T : class
        {
            string val = o.GetPropVal(propName)?.ToString();
            return val.Trim();
        }
    }
}