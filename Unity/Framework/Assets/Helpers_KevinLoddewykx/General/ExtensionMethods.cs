using System.Collections.Generic;
using System.Linq;

namespace Helpers_KevinLoddewykx.General
{
    public static class ExtensionMethods
    {
        public static void Resize<T>(this List<T> list, int size) where T : new()
        {
            int count = list.Count;

            if (size < count)
            {
                list.RemoveRange(size, count - size);
            }
            else if (size > count)
            {
                if (size > list.Capacity)
                {
                    list.Capacity = size;
                }

                list.AddRange(Enumerable.Repeat(new T(), size - count));
            }
        }

        public static void InitNew<T>(this T[] array) where T : new()
        {
            for (int i = 0; i < array.Length; ++i)
            {
                array[i] = new T();
            }
        }
    }
}

