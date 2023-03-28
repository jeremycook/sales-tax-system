using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace SiteKit.EntityFrameworkCore
{
    public static class SiteKitEntityFrameworkCoreModelBuilderExtensions
    {
        /// <summary>
        /// The <see cref="Metadata.IMutableEntityType"/>s of the <see cref="ModelBuilder.Model"/>
        /// will be scanned and the <see cref="IEntityTypeConfiguration{TEntity}"/> from their
        /// assemblies will be applied.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void ApplyConfigurationsFromEntityTypeAssemblies(this ModelBuilder modelBuilder)
        {
            foreach (var assembly in modelBuilder.Model
                .GetEntityTypes()
                .Select(et => et.ClrType.Assembly)
                .Distinct()
                .ToArray())
            {
                modelBuilder.ApplyConfigurationsFromAssembly(assembly);
            }
        }

        /// <summary>
        /// Apply instances of <see cref="IModelBuilderContributor"/> from the <paramref name="assemblies"/>
        /// to the <paramref name="modelBuilder"/>.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void ApplyModelBuilderContributors(this ModelBuilder modelBuilder, params Assembly[] assemblies)
        {
            var mbcType = typeof(IModelBuilderContributor);
            foreach (var type in assemblies.SelectMany(a => a.ExportedTypes))
            {
                if (mbcType.IsAssignableFrom(type))
                {
                    var mbc = (IModelBuilderContributor)Activator.CreateInstance(type);
                    mbc.ContributeTo(modelBuilder);
                }
            }
        }

        /// <summary>
        /// Each entity type's schema will be set to the last part of the namespace of its CLR type.
        /// Shadow types will not be modified.
        /// Call this after all entities have been added to the <see cref="DbContext"/>.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void ApplySchemaBasedOnNamespace(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (entityType.HasTable() && entityType.HasDefaultSchema())
                {
                    entityType.SetSchema(entityType.ClrType.Namespace.Substring(entityType.ClrType.Namespace.LastIndexOf('.') + 1));
                }
            }
        }

        /// <summary>
        /// Each entity type's schema will be set to the last part of the <see cref="AssemblyName.Name"/> of its CLR type.
        /// Shadow types will not be modified.
        /// If your assembly is named "MyAssembly.Models" the schema will be set to "Models".
        /// The recommended approach is to name your assembly "MyAssembly" and place your entities in a Models folder.
        /// Call this after all entities have been added to the <see cref="DbContext"/>.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void ApplySchemaBasedOnAssembly(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (entityType.HasTable() && entityType.HasDefaultSchema())
                {
                    var assemblyName = entityType.ClrType.Assembly.GetName().Name;
                    entityType.SetSchema(assemblyName.Substring(assemblyName.LastIndexOf('.') + 1));
                }
            }
        }

        /// <summary>
        /// Call this after all entities have been added to the <see cref="DbContext"/>.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void ApplySingularTableNames(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (entityType.HasTable() && entityType.HasDefaultTableName())
                {
                    entityType.SetTableName(entityType.DisplayName());
                }
            }
        }

        /// <summary>
        /// Converts all schema, table and property names to underscore style.
        /// Example: MySchema.MyTable.MyField becomes my_schema.my_table.my_field.
        /// Call this after all entities have been added to the <see cref="DbContext"/>.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void ApplyUnderscoreNames(this ModelBuilder modelBuilder, bool force = false)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (entityType.HasTable())
                {
                    if (entityType.HasDefaultTableName() || force)
                        entityType.SetTableName(entityType.GetTableName().Underscore());
                    if (entityType.HasDefaultSchema() || force)
                        entityType.SetSchema(entityType.GetSchema().Underscore());
                }

                var soid =
                    StoreObjectIdentifier.Create(entityType, entityType.GetSqlQuery() != null ? StoreObjectType.SqlQuery : StoreObjectType.Table) ??
                    throw new InvalidOperationException($"The {nameof(StoreObjectIdentifier)} could not be determined for {entityType.Name} entity type.");

                foreach (var property in entityType.GetProperties())
                {
                    var columnName = property.GetColumnName(soid) ?? property.GetDefaultColumnName(soid);
                    property.SetColumnName(columnName.Underscore());
                }
            }
        }

        public static void ApplyDecimalColumnType(this ModelBuilder modelBuilder, string columnType = "decimal(18,2)")
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (typeof(decimal?).IsAssignableFrom(property.ClrType) &&
                        property.GetColumnType() == null)
                    {
                        property.SetColumnType(columnType);
                    }
                }
            }
        }

        public static void ApplyDateColumnType(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (typeof(DateTime?).IsAssignableFrom(property.ClrType) &&
                        property.PropertyInfo.GetCustomAttribute<DataTypeAttribute>()?.DataType == DataType.Date &&
                        property.GetColumnType() == null)
                    {
                        property.SetColumnType("date");
                    }
                }
            }
        }
    }
}
