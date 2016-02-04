using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fairytale.Extensions
{
    public static class StringExtensions
    {
        public static bool Contains(this string self, char character)
        {
            foreach (var ch in self)
            {
                if (ch == character)
                    return true;
            }

            return false;
        }
    }
}
