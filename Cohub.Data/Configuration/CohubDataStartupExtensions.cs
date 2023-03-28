using Cohub.Data.Fin.Deposits;
using Cohub.Data.Fin.Returns;
using Cohub.Data.Fin.Statements;
using Cohub.Data.Fin.Batches;
using Cohub.Data.Geo.Configuration;
using Cohub.Data.Ins;
using Cohub.Data.Pg;
using Cohub.Data.Sto.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SiteKit;
using SiteKit.DependencyInjection;
using SiteKit.EntityFrameworkCore;
using System;
using Cohub.Data.Org;

namespace Cohub.Data.Configuration
{
    public static class CohubDataStartupExtensions
    {
        public static IServiceCollection AddCohubData(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            services.Configure<CohubDataOptions>(configuration.GetSection("CohubData"));
            var cohubDataOptions = configuration.GetSection("CohubData").Get<CohubDataOptions>();

            // DbContext
            services.AddDbContext<CohubDbContext>(options => options
                .UseNpgsql(configuration.GetConnectionString("cohub_npgsql"), b => b.MigrationsAssembly("Cohub.Data.PostgreSQL"))
                .AddInterceptors(new AsyncDbConnectionInterceptor(async (connection, connectionEndEventData, cancellationToken) =>
                {
                    if (cohubDataOptions.OwnerRole is string role && !role.IsNullOrWhiteSpace())
                    {
                        var cmd = connection.CreateCommand();
                        cmd.CommandText = $"SET ROLE {role};";
                        await cmd.ExecuteNonQueryAsync(cancellationToken);
                    }
                }))
                .ConfigureWarnings(w =>
                {
                    if (environment.IsProduction())
                        w.Log(RelationalEventId.MultipleCollectionIncludeWarning);
                    else
                        w.Throw(RelationalEventId.MultipleCollectionIncludeWarning);
                }));
            services.AddSingleton<Singleton<CohubDbContext>>();
            services.AddDbContext<CohubReadDbContext>(options => options
                .UseNpgsql(configuration.GetConnectionString("read_npgsql"))
                .AddInterceptors(new AsyncDbConnectionInterceptor(async (connection, connectionEndEventData, cancellationToken) =>
                {
                    if (cohubDataOptions.ReadRole is string role && !role.IsNullOrWhiteSpace())
                    {
                        var cmd = connection.CreateCommand();
                        cmd.CommandText = $"SET ROLE {role};";
                        await cmd.ExecuteNonQueryAsync(cancellationToken);
                    }
                })));

            // Shared
            services.AddAttributedServicesFromAssemblies(typeof(CohubDataStartupExtensions).Assembly);

            // Org
            services.AddScoped<OrganizationExpiry>();

            // Fin
            services.AddScoped<ReturnGenerator>();
            services.AddScoped<ReturnRefresher>();
            services.AddScoped<DueCalculator>();
            services.AddScoped<IInterestCalculator, NextDayInterestCalculator>();
            services.AddScoped<StatementCalculator>();
            services.AddScoped<DepositService>();
            services.AddScoped<TransferMoneyService>();
            services.AddScoped<TransferReturnService>();

            // Geo
            services.Configure<GeoOptions>(configuration.GetSection("Geo"));

            // Insights
            services.AddDbContext<InsightsDesignerDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("insights_designer_npgsql")));

            // Sto
            services.Configure<StoOptions>(configuration.GetSection("Sto"));

            return services;
        }
    }
}
