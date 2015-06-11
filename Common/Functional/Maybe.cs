using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System
{
    /// <summary>
    /// A simple container for a possible value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [JsonConverter(typeof(MaybeJsonConverter))]
    public struct Maybe<T>
    {
        private T _value;
        private bool _hasValue;

        public Maybe(T value)
        {
            _value = value;
            _hasValue = true;
        }

        public T Value { get { if (!_hasValue) throw new NullReferenceException(); return _value; } }
        public bool HasValue { get { return _hasValue; } }

        public T ValueOrException(Func<Exception> makeException)
        {
            if (!_hasValue) throw makeException();
            return _value;
        }

        public T GetValueOrDefault()
        {
            if (!_hasValue) return default(T);
            return _value;
        }

        public T GetValueOrDefault(Thunk<T> defaultValue)
        {
            if (!_hasValue) return (T)defaultValue;
            return _value;
        }

        public U Either<U>(Func<U> noValue, Func<T, U> hasValue)
        {
            if (_hasValue) return hasValue(_value);
            else return noValue();
        }

        public static implicit operator Maybe<T>(T value)
        {
            return new Maybe<T>(value);
        }

        public static readonly Maybe<T> Nothing = new Maybe<T>();
    }

    /// <summary>
    /// A Newtonsoft.Json JsonConverter implementation to serialize and deserialize strongly-typed IDs.
    /// </summary>
    public sealed class MaybeJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            // We can only convert `Maybe<T>` to JSON:
            return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Maybe<>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Debug.Assert(objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Maybe<>));

            if (reader.TokenType == JsonToken.Null)
                return Activator.CreateInstance(objectType);

            // Deserialize the JSON of the inner type `T` of `Maybe<T>`:
            var obj = serializer.Deserialize(reader, objectType.GetGenericArguments()[0]);
            var id = Activator.CreateInstance(objectType, obj);
            return id;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var objectType = value.GetType();
            Debug.Assert(objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Maybe<>));

            // If the `Maybe<T>` has no value then serialize a null:
            if (!(bool)objectType.GetProperty("HasValue").GetValue(value))
            {
                writer.WriteNull();
                return;
            }

            // Get the inner value of the `Maybe<T>` and serialize that:
            var innerValue = objectType.GetProperty("Value").GetValue(value);
            serializer.Serialize(writer, innerValue);
        }
    }
}
