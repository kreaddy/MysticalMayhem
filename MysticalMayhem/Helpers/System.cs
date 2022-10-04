using System;
using System.Collections.Generic;
using System.Linq;

namespace MysticalMayhem.Helpers
{
    internal static class System
    {
        internal static T[] Push<T>(this T[] _instance, params T[] elements)
        {
            var list = _instance.ToList();
            list.AddRange(elements);
            return list.ToArray();
        }

        internal static T[] Pop<T>(this T[] _instance)
        {
            var list = _instance.ToList();
            list.RemoveAt(0);
            return list.ToArray();
        }

        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            foreach (T item in sequence)
            {
                action(item);
            }
        }
    }
}