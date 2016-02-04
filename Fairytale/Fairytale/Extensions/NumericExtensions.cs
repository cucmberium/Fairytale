using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fairytale.Extensions
{
    public static class NumericExtensions
    {
        public static object Cast<T>(string s)
        {
            var typeInfo = typeof(T);

            if (typeInfo == typeof(long)) return long.Parse(s);
            if (typeInfo == typeof(double)) return double.Parse(s);
            if (typeInfo == typeof(int)) return int.Parse(s);
            if (typeInfo == typeof(float)) return float.Parse(s);
            if (typeInfo == typeof(short)) return short.Parse(s);
            if (typeInfo == typeof(ushort)) return ushort.Parse(s);
            if (typeInfo == typeof(uint)) return uint.Parse(s);
            if (typeInfo == typeof(ulong)) return ulong.Parse(s);
            if (typeInfo == typeof(byte)) return byte.Parse(s);
            if (typeInfo == typeof(decimal)) return decimal.Parse(s);
            if (typeInfo == typeof(sbyte)) return sbyte.Parse(s);

            throw new InvalidCastException();
        }
    }
}
