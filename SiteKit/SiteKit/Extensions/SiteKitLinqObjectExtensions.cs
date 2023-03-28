using System.Collections.Generic;

namespace System.Linq
{
    public static class SiteKitLinqObjectExtensions
    {
        /// <summary>
        /// Flatten a hierarchy of <typeparamref name="T"/> into a list.
        /// Breaks if the item is <c>null</c>, otherwise the first item will be <paramref name="root"/>.
        /// Breaks if <paramref name="root"/> is found by the <paramref name="flattener"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="root"></param>
        /// <param name="flattener"></param>
        /// <returns></returns>
        public static IEnumerable<T> Flatten<T>(this T root, Func<T, T> flattener)
            where T : class
        {
            if (root == null)
            {
                yield break;
            }

            yield return root;

            var current = flattener(root);
            while (current != null && current != root)
            {
                yield return current;
                current = flattener(current);
            }
        }

        /// <summary>
        /// Apply an <paramref name="action"/> for each <typeparamref name="TItem"/> in <paramref name="items"/>.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="items"></param>
        /// <param name="action"></param>
        public static void ForEach<TItem>(this IEnumerable<TItem> items, Action<TItem> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }
    }
}
