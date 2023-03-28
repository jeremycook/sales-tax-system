using System;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore
{
    public static class SiteKitEntityFrameworkCoreDbSetExtensions
    {
        public static async Task<TEntity> FindOrAddAsync<TEntity, TId>(this DbSet<TEntity> dbSet, TId id, Func<TEntity> activator)
            where TEntity : class
        {
            var entity = await dbSet.FindAsync(id);

            if (entity is null)
            {
                entity = activator();
                dbSet.Add(entity);
            }

            return entity;
        }
    }
}
