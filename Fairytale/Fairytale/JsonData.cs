using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using Fairytale.Extensions;

namespace Fairytale
{
    public enum JsonDataType
    {
        Numeric = 0,
        String = 1,
        Boolean = 2,
        Array = 3,
        Object = 4,
        Null = 5,
    }

    public class JsonData : IEnumerable<JsonData>, IReadOnlyDictionary<string, JsonData>
    {
        public JsonData()
        {
            _Childs = new List<JsonData>();
        }
        
        public JsonDataType Type { get; internal set; }
        
        public T GetValue<T>()
        {
            if (Type == JsonDataType.Numeric)
                return (T)NumericExtensions.Cast<T>(_Value as string);
            else
                return (T)_Value;
        }
        public string GetKey()
        {
            return _Key;
        }

        internal List<JsonData> _Childs;
        internal object _Value;
        internal string _Key;

        
        IEnumerator<JsonData> IEnumerable<JsonData>.GetEnumerator()
        {
            return _Childs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Childs.GetEnumerator();
        }

        IEnumerable<string> IReadOnlyDictionary<string, JsonData>.Keys
        {
            get
            {
                return _Childs.Select(x => x.GetKey());
            }
        }

        IEnumerable<JsonData> IReadOnlyDictionary<string, JsonData>.Values
        {
            get
            {
                return _Childs;
            }
        }

        int IReadOnlyCollection<KeyValuePair<string, JsonData>>.Count
        {
            get
            {
                return _Childs.Count;
            }
        }

        JsonData IReadOnlyDictionary<string, JsonData>.this[string key]
        {
            get
            {
                return _Childs.First(x => x._Key == key);
            }
        }

        bool IReadOnlyDictionary<string, JsonData>.ContainsKey(string key)
        {
            return _Childs.Any(x => x._Key == key);
        }

        bool IReadOnlyDictionary<string, JsonData>.TryGetValue(string key, out JsonData value)
        {
            throw new NotImplementedException();
        }

        IEnumerator<KeyValuePair<string, JsonData>> IEnumerable<KeyValuePair<string, JsonData>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public JsonData this[string key]
        {
            get
            {
                return _Childs.First(x => x._Key == key);
            }
        }

        public JsonData this[int key]
        {
            get
            {
                return _Childs.ElementAt(key);
            }
        }
    }
}
