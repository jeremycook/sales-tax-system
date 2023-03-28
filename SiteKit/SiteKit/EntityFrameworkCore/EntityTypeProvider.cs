using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using SiteKit.DependencyInjection;
using System;
using System.Linq;

namespace SiteKit.EntityFrameworkCore
{
    /// <summary>
    /// (Singleton) A general purpose way to get the <see cref="IEntityType"/>
    /// without knowing the <see cref="DbContext"/> it belongs to.
    /// The first <see cref="DbContext"/> that can provide the information wins.
    /// </summary>
    public class EntityTypeProvider
    {
        private readonly SingletonScope serviceProvider;
        private readonly CandidateDbContextTypes candidateDbContextTypes;

        public EntityTypeProvider(SingletonScope serviceProvider, CandidateDbContextTypes candidateDbContextTypes)
        {
            this.serviceProvider = serviceProvider;
            this.candidateDbContextTypes = candidateDbContextTypes;
        }

        public IEntityType GetEntityType(Type type)
        {
            return candidateDbContextTypes
                .Select(type => (DbContext)serviceProvider.ServiceProvider.GetRequiredService(type))
                .Select(db => db.Model.FindEntityType(type))
                .First(et => et != null);
        }
    }
}
