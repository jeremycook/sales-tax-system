using System.Linq;

namespace System.Collections.Generic
{
    public static class SiteKitCollectionsExtensions
    {
        /// <summary>
        /// Adds the <paramref name="items"/> to this <paramref name="collection"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="items"></param>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (collection is List<T> list)
            {
                list.AddRange(items);
            }
            else
            {
                foreach (var item in items)
                {
                    collection.Add(item);
                }
            }
        }

        /// <summary>
        /// Merge <paramref name="items"/> into this <paramref name="collection"/>, adding and removing as needed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="items"></param>
        /// <param name="matcher">The left <typeparamref name="T"/> comes from the <paramref name="collection"/> and the right <typeparamref name="T"/> is comes from <paramref name="items"/>.</param>
        public static void UpdateWithRange<T>(this ICollection<T> collection, IEnumerable<T> items)
            where T : class, IEquatable<T>
        {
            collection.UpdateWithRange(
                items,
                (source, target) => source == target,
                creator: source => source);
        }

        /// <summary>
        /// Merge <paramref name="items"/> into this <paramref name="collection"/>, adding and removing as needed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="items"></param>
        /// <param name="matcher">The left <typeparamref name="T"/> comes from the <paramref name="collection"/> and the right <typeparamref name="T"/> is comes from <paramref name="items"/>.</param>
        public static void UpdateWithRange<T>(this ICollection<T> collection, IEnumerable<T> items, Func<T, T, bool> matcher)
        {
            collection.UpdateWithRange(
                items,
                matcher,
                creator: source => source);
        }

        /// <summary>
        /// Merge <paramref name="items"/> into this <paramref name="collection"/>, adding and removing as needed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="items"></param>
        /// <param name="matcher"></param>
        public static void UpdateWithRange<TSource, TTarget>(this ICollection<TTarget> collection, IEnumerable<TSource> items, Func<TSource, TTarget, bool> matcher, Action<TSource, TTarget> updater)
            where TSource : TTarget
        {
            UpdateWithRange(collection,
                items,
                matcher,
                creator: source => source,
                updater: updater,
                remover: null);
        }

        /// <summary>
        /// Merge <paramref name="items"/> into this <paramref name="collection"/>, creating, updating and removing as needed.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="collection"></param>
        /// <param name="items"></param>
        /// <param name="matcher"></param>
        /// <param name="creator"></param>
        /// <param name="updater"></param>
        /// <param name="remover"></param>
        public static void UpdateWithRange<TSource, TTarget>(this ICollection<TTarget> collection, IEnumerable<TSource> items, Func<TSource, TTarget, bool> matcher, Func<TSource, TTarget> creator, Action<TSource, TTarget>? updater = null, Action<TTarget>? remover = null)
        {
            if (collection is null)
                throw new ArgumentNullException(nameof(collection));
            if (items is null)
                throw new ArgumentNullException(nameof(items));

            var matched = new List<TTarget>();
            var added = new List<TTarget>();
            foreach (var source in items)
            {
                if (collection.Any(target => matcher(source, target)))
                {
                    var matchedTarget = collection.FirstOrDefault(target => matcher(source, target));
                    updater?.Invoke(source, matchedTarget);
                    matched.Add(matchedTarget);
                }
                else
                {
                    var newTarget = creator(source);
                    added.Add(newTarget);
                }
            }
            collection.AddRange(added);

            // Removals only exist in collection and are missing from items.
            foreach (var item in collection.Except(matched).Except(added).ToArray())
            {
                if (remover != null)
                {
                    remover(item);
                }
                else
                {
                    collection.Remove(item);
                }
            }
        }
    }
}
