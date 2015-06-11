using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace System
{
    public static class ObjectExtensions
    {
#if false
        /// <summary>
        /// Clones all properties defined in the <typeparamref name="Tinterface"/> from the source object to this object.
        /// </summary>
        /// <param name="target">Destination object whose property values will be set</param>
        /// <param name="source">Source object whose property values will be retrieved</param>
        /// <param name="throwExceptions">If true, throw the first exception encountered during property
        /// method invocation. If false, silently eat all exceptions and continue on processing all properties</param>
        /// <typeparam name="Tinterface">Interface type which both objects must implement which defines a set
        /// of properties whose values will be cloned from source to this.</typeparam>
        public static void CloneByInterface<Tinterface>(this Tinterface target, Tinterface source, bool throwExceptions)
        {
            // TODO: consider locking the target object somehow?

            Type interfaceType = typeof(Tinterface);

            // Does the source object type implement ICloneableTo<Tinterface> ?
            Type cloneableToType = typeof(ICloneableTo<>).MakeGenericType(typeof(Tinterface));
            if (cloneableToType.IsAssignableFrom(source.GetType()))
            {
                // The source object implements the ICloneableTo<Tinterface> interface.
                ICloneableTo<Tinterface> cloneable = (ICloneableTo<Tinterface>)source;
                cloneable.CloneTo(target);
                return;
            }

            // For each property defined on the interface type, copy the value from 'source' to 'target'.
            foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(interfaceType))
            {
                if (pd.IsReadOnly) continue;
                try
                {
                    object value = pd.GetValue(source);
                    pd.SetValue(target, value);
                }
                catch
                {
                    // Failed to get or set the property value.  We don't really care which.
                    if (!throwExceptions)
                        continue;
                    throw;
                }
            }
        }

        /// <summary>
        /// Clones all properties defined in the <typeparamref name="Tinterface"/> from the source object to this object.
        /// This overload eats all exceptions encountered during property cloning and guarantees progress through
        /// the entire set of properties.
        /// </summary>
        /// <param name="target">Destination object whose property values will be set</param>
        /// <param name="source">Source object whose property values will be retrieved</param>
        /// <typeparam name="Tinterface">Interface type which both objects must implement which defines a set
        /// of properties whose values will be cloned from source to this.</typeparam>
        public static void CloneByInterface<Tinterface>(this Tinterface target, Tinterface source)
        {
            target.CloneByInterface<Tinterface>(source, false);
        }
#endif
        public static bool IsNullOrDefault(this object obj)
        {
            bool ret = true;

            if (obj is string)
            {
                ret = String.IsNullOrEmpty((string)obj);
            }
            else if (obj is int)
            {
                ret = ((int)obj == default(int));
            }
            else if (obj is Enum)
            {
                Enum enumObjVal = obj as Enum;
                int intEnumVal = (int)Enum.ToObject(enumObjVal.GetType(), enumObjVal);
                ret = intEnumVal == default(int);
            }
            else
            {
                ret = obj == null;
            }

            return ret;
        }


        /// <summary>
        /// Thread-safely initialize an object reference.
        /// </summary>
        /// <typeparam name="T">Type of the reference to be initialized</typeparam>
        /// <param name="x"></param>
        /// <param name="reference">A reference to the object to initialize</param>
        /// <param name="lockObject">An object reference used for synchronization</param>
        /// <param name="initialize">An initializer function to create the reference</param>
        /// <returns></returns>
        public static T ThreadSafeInitialize<T>(this Type x, ref T reference, object lockObject, Func<T> initialize)
            where T : class
        {
            if (lockObject == null) throw new ArgumentNullException("lockObject");
            if (initialize == null) throw new ArgumentNullException("initialize");

            if (reference == null)
            {
                lock (lockObject)
                {
                    if (reference == null)
                    {
                        reference = initialize();
                    }
                }
            }
            
            return reference;
        }

        /// <summary>
        /// Prepends items in <paramref name="prepend"/> to this enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">current enumerable to prepend to</param>
        /// <param name="prepend">enumerable to prepend</param>
        /// <returns></returns>
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, IEnumerable<T> prepend)
        {
            foreach (T item in prepend)
            {
                yield return item;
            }
            foreach (T item in source)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Appends items in <paramref name="append"/> to this enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">current enumerable to append to</param>
        /// <param name="append">enumerable to append</param>
        /// <returns></returns>
        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, IEnumerable<T> append)
        {
            foreach (T item in source)
            {
                yield return item;
            }
            foreach (T item in append)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Checks if the target object is null. If not, the selected member's value is returned,
        /// otherwise <code>default(<typeparamref name="U"/>)</code> is returned.
        /// </summary>
        /// <typeparam name="T">Type of the object to check against null</typeparam>
        /// <typeparam name="U">Type of the object's member to select</typeparam>
        /// <param name="obj">Source object to check against null reference</param>
        /// <param name="memberAccess">Function to access the member (e.g. <code>x => x.MemberName</code>)</param>
        /// <returns></returns>
        /// <remarks>This extension method implements a would-be-nice-to-have operator in C#, generally
        /// referred to as the proposed <code>?.</code> operator.</remarks>
        public static U NullSafe<T, U>(this T obj, Func<T, U> memberAccess)
            where T : class
        {
            if (obj == null) return default(U);

            return memberAccess(obj);
        }

        /// <summary>
        /// Checks if the target object is null. If not, the selected member's value is returned,
        /// otherwise <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <typeparam name="T">Type of the object to check against null</typeparam>
        /// <typeparam name="U">Type of the object's member to select</typeparam>
        /// <param name="obj">Source object to check against null reference</param>
        /// <param name="memberAccess">Function to access the member (e.g. <code>x => x.MemberName</code>)</param>
        /// <param name="defaultValue">Default value to return if the object is null</param>
        /// <returns></returns>
        /// <remarks>This extension method implements a would-be-nice-to-have operator in C#, generally
        /// referred to as the proposed <code>?.</code> operator.</remarks>
        public static U NullSafe<T, U>(this T obj, Func<T, U> memberAccess, U defaultValue)
            where T : class
        {
            if (obj == null) return defaultValue;

            return memberAccess(obj);
        }
    }
}
