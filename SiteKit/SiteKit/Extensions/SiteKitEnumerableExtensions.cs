using System.ComponentModel;
using System.Data;
using System.Linq;

namespace System.Collections.Generic
{
    public static class SiteKitEnumerableExtensions
    {
        /// <summary>
        /// If <see cref="enumerable"/> is <c>null</c> and empty <see cref="IEnumerable{T}"/>
        /// will be returned, otherwise <paramref name="enumerable"/> is returned as-is.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static IEnumerable<T> OrEmptyEnumerable<T>(this IEnumerable<T>? enumerable)
        {
            return enumerable ?? Enumerable.Empty<T>();
        }

        /// <summary>
        /// Returns a <see cref="DataTable"/> filled with data from <paramref name="enumerable"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> enumerable)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));

            DataTable table = new DataTable();

            foreach (PropertyDescriptor prop in properties)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (T item in enumerable)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }

            return table;
        }
    }
}
