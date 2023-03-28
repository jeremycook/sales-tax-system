using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore
{
    public static class SiteKitEntityFrameworkCoreEntityTypeExtensions
    {
        public static TAttribute? GetCustomAttribute<TAttribute>(this IEntityType entityType)
            where TAttribute : Attribute
        {
            return entityType.ClrType.GetCustomAttribute<TAttribute>();
        }

        public static string SingluarName(this IEntityType entityType)
        {
            return entityType.GetCustomAttribute<DisplayAttribute>()?.Name ?? entityType.DisplayName();
        }

        public static string PluralName(this IEntityType entityType)
        {
            return entityType.GetCustomAttribute<DisplayAttribute>()?.GroupName ?? entityType.DisplayName().Pluralize();
        }

        /// <summary>
        /// Returns <c>true</c> if the <paramref name="entityType"/> has a table.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static bool HasTable(this IEntityType entityType)
        {
            return
                entityType.ClrType != null &&
                entityType.BaseType == null &&
                entityType.DefiningEntityType == null && 
                entityType.GetSqlQuery() == null;
        }

        /// <summary>
        /// Returns <c>true</c> if the <paramref name="entityType"/> has the default table name.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static bool HasDefaultTableName(this IEntityType entityType)
        {
            return entityType.GetDefaultTableName() == entityType.GetTableName();
        }

        /// <summary>
        /// Returns <c>true</c> if the <paramref name="entityType"/> has the default schema name.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static bool HasDefaultSchema(this IEntityType entityType)
        {
            return entityType.GetDefaultSchema() == entityType.GetSchema();
        }

        /// <summary>
        /// Cast the values of <paramref name="id"/> to the correct types based on <paramref name="key"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static object[] Cast(this IKey key, object[] id)
        {
            var props = key.Properties;

            var result = new object[props.Count];

            for (int i = 0; i < props.Count; i++)
            {
                var prop = props[i];
                object value = id[i];
                if (value.GetType() == prop.ClrType)
                {
                    result[i] = value;
                }
                else if (prop.ClrType == typeof(Guid))
                {
                    result[i] = Guid.Parse(value.ToString());
                }
                else
                {
                    result[i] = Convert.ChangeType(value, prop.ClrType);
                }
            }

            return result;
        }
    }
}
