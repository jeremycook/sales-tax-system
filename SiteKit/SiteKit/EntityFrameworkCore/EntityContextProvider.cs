using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace SiteKit.EntityFrameworkCore
{
    /// <summary>
    /// (Scoped) Get the <see cref="DbContext"/> or <see cref="EntityEntry"/> of an entity.
    /// </summary>
    public class EntityContextProvider
    {
        private readonly IServiceProvider serviceProvider;
        private readonly CandidateDbContextTypes candidateDbContextTypes;

        public EntityContextProvider(IServiceProvider serviceProvider, CandidateDbContextTypes candidateDbContextTypes)
        {
            this.serviceProvider = serviceProvider;
            this.candidateDbContextTypes = candidateDbContextTypes;
        }

        public DbContext GetDbContext(Type type)
        {
            return candidateDbContextTypes
                .Select(type => (DbContext)serviceProvider.GetRequiredService(type))
                .Where(db => db.Model.FindEntityType(type) != null)
                .First();
        }

        public IEntityType GetEntityType(Type type)
        {
            return candidateDbContextTypes
                .Select(type => (DbContext)serviceProvider.GetRequiredService(type))
                .Select(db => db.Model.FindEntityType(type))
                .First(et => et != null);
        }

        public EntityEntry GetEntityEntry(object entity)
        {
            return GetDbContext(entity.GetType()).Entry(entity);
        }
    }
}
