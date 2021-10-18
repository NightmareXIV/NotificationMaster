using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationMaster
{
    static class Static
    {
        public static void AddShifting<T>(this T[] array, T element)
        {
            for(var i = array.Length - 2; i >= 0; i--)
            {
                array[i + 1] = array[i];
            }
            array[0] = element;
        }

        public static string NotNull(this string s)
        {
            return s == null ? "" : s;
        }
    }
}
