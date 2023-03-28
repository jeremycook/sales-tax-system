using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SiteKit.DependencyInjection;
using SiteKit.EntityFrameworkCore;
using SiteKit.Info;
using System.Linq;
using System.Reflection;

namespace SiteKit
{
    public static class SiteKitStartupExtensions
    {
        public static IServiceCollection AddSiteKit(this IServiceCollection services, IConfiguration configuration)
        {
            // About
            services.Configure<AboutOptions>(configuration.GetSection("SiteKit:About"));
            services.AddScoped<Actor>();

            // Dependency injection
            services.AddSingleton<SingletonScope>();

            // Entity Framework Core
            services.AddSingleton<EntityTypeProvider>();
            services.AddScoped<EntityContextProvider>();
            services.AddScoped<EntityUrlHelper>();

            return services;
        }

        /// <summary>
        /// Adds types from the <paramref name="assemblies"/> with the <see cref="ScopedAttribute"/>
        /// to the <paramref name="services"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IServiceCollection AddAttributedServicesFromAssemblies(this IServiceCollection services, params Assembly[] assemblies)
        {
            foreach (var type in assemblies.SelectMany(a => a.ExportedTypes))
            {
                if (type.GetCustomAttribute<ScopedAttribute>() is ScopedAttribute scopedAttribute)
                {
                    services.AddScoped(scopedAttribute.ServiceType ?? type, type);
                }
            }

            return services;
        }
    }
}
