using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SiteKit.Info;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteKit.EntityFrameworkCore
{
    /// <summary>
    /// (Scoped) A URL generate for entities of a <see cref="DbContext"/>.
    /// Uses <see cref="EntityTypeProvider"/> to grab information based on the entity or <see cref="Type"/> of an entity.
    /// </summary>
    public class EntityUrlHelper
    {
        private static readonly Dictionary<Type, string> baseUrlDictionary = new();

        private readonly Uri baseUri;
        private readonly EntityContextProvider entityContextProvider;

        public EntityUrlHelper(IOptions<AboutOptions> aboutOptions, EntityContextProvider entityContextProvider)
        {
            baseUri = new Uri(aboutOptions.Value.BaseUrl);
            this.entityContextProvider = entityContextProvider;
        }

        public Uri List<TEntity>()
        {
            var entityType = entityContextProvider.GetEntityType(typeof(TEntity));
            return new Uri(baseUri, GetBaseUrl(entityType));
        }

        public Uri Action(object entity, string action)
        {
            var entry = entityContextProvider.GetEntityEntry(entity);
            var entityType = entry.Metadata;
            var pk = entityType.FindPrimaryKey();
            return new Uri(baseUri, $"{GetBaseUrl(entityType)}/{action}/{string.Join("/", pk.Properties.Select(p => entry.Property(p.Name).CurrentValue))}");
        }

        public Uri Item(Type eType, params object[] primaryKey)
        {
            if (primaryKey.Length == 0)
            {
                throw new ArgumentException("The array must contain at least one value.", nameof(primaryKey));
            }

            var entityType = entityContextProvider.GetEntityType(eType);
            return new Uri(baseUri, $"{GetBaseUrl(entityType)}/{string.Join("/", primaryKey)}");
        }

        public Uri Item(object entity)
        {
            var entry = entityContextProvider.GetEntityEntry(entity);
            var entityType = entry.Metadata;
            var pk = entityType.FindPrimaryKey();
            return new Uri(baseUri, $"{GetBaseUrl(entityType)}/{string.Join("/", pk.Properties.Select(p => entry.Property(p.Name).CurrentValue))}");
        }

        private static string GetBaseUrl(Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType)
        {
            if (!baseUrlDictionary.TryGetValue(entityType.ClrType, out string url))
            {
                var area = entityType.ClrType.Namespace.Split('.').Last().Underscore().Dasherize();

                url = $"{area}/{entityType.ClrType.Name.Pluralize().Underscore().Dasherize()}";

                baseUrlDictionary[entityType.ClrType] = url;
            }

            return url;
        }
    }
}
