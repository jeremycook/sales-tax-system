using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore
{
    public static class SiteKitEntityFrameworkCoreModelExtensions
    {
        /// <summary>
        /// Gets the entity that maps the given entity class. Returns null if no entity type
        /// with the given CLR type is found or the given CLR type is being used by shared
        /// type entity type or the entity type has a defining navigation.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public static IEntityType FindEntityType<TEntity>(this IModel model)
        {
            return model.FindEntityType(typeof(TEntity));
        }
    }
}
