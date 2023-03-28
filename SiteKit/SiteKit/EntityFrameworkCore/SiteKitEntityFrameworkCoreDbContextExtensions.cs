using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.EntityFrameworkCore
{
    public static class SiteKitEntityFrameworkCoreDbContextExtensions
    {
        /// <summary>
        /// Adds the <paramref name="entities"/> to <paramref name="dbContext"/>
        /// if a record matching record does not already exist.
        /// Yields entities that were added.
        /// </summary>
        /// <typeparam name="DbContext"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static List<TEntity> AddMissingRange<TEntity>(this DbContext dbContext, IEnumerable<TEntity> entities)
            where TEntity : class
        {
            var pk = dbContext.Model.FindEntityType<TEntity>().FindPrimaryKey();

            if (entities.Select(e => string.Join("|", pk.Properties.Select(p => p.GetGetter().GetClrValue(e)))).Distinct().Count() != entities.Count())
            {
                throw new ArgumentException("Duplicate Ids were found in entities.", nameof(entities));
            }

            IEnumerable<TEntity> invalidEntities = entities.Where(e => pk.Properties.Any(p => p.GetGetter().GetClrValue(e) == default));
            if (invalidEntities.Any())
            {
                throw new ArgumentException($"Invalid entities were found: {System.Text.Json.JsonSerializer.Serialize(invalidEntities)}.", nameof(entities));
            }

            var list = new List<TEntity>();
            foreach (var entity in entities)
            {
                object[] keyValues = pk.Properties.Select(p => p.GetGetter().GetClrValue(entity)).ToArray();

                if (dbContext.Find<TEntity>(keyValues: keyValues) == default(TEntity))
                {
                    dbContext.Add(entity);
                    list.Add(entity);
                }
            }
            return list;
        }

        public static dynamic Set(this DbContext dbContext, Type entityType)
        {
            System.Reflection.MethodInfo setMethod = dbContext.GetType().GetMethod(nameof(dbContext.Set), 1, Array.Empty<Type>()).MakeGenericMethod(entityType);
            dynamic dbSet = setMethod.Invoke(dbContext, null);
            return dbSet;
        }
    }
}
