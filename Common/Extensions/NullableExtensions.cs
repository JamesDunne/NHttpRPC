using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public static class NullableExtensions
    {
        public static Nullable<U> Convert<T, U>(this Nullable<T> nullable, Converter<T, U> convert)
            where T : struct
            where U : struct
        {
            if (!nullable.HasValue) return null;
            return convert(nullable.Value);
        }

        /// <summary>
        /// Formats a nullable value as an empty string if it is null, otherwise uses default ToString() method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nullableValue"></param>
        /// <returns></returns>
        public static string ToEmptyIfNullString<T>(this Nullable<T> nullableValue)
            where T : struct
        {
            if (!nullableValue.HasValue) return String.Empty;

            return nullableValue.Value.ToString();
        }

        /// <summary>
        /// Formats a nullable value as an empty string if it is null, otherwise uses provided format method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nullableValue"></param>
        /// <returns></returns>
        public static string NullFormat<T>(this Nullable<T> nullableValue, Func<T, string> toString)
            where T : struct
        {
            if (!nullableValue.HasValue) return String.Empty;

            toString = toString ?? ((v) => v.ToString());
            return toString(nullableValue.Value);
        }
    }
}
