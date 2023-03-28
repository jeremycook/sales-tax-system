using System;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Cohub.Data.Scaffolder
{
    public class ScaffoldingDesignTimeServices : IDesignTimeServices
    {
        public void ConfigureDesignTimeServices(IServiceCollection services)
        {
            services
                .AddSingleton<ICandidateNamingService, CustomCandidateNamingService>();
            // TODO: Try out Handlebars EF Scaffolding when .NET 5 is supported
            // services.AddHandlebarsScaffolding(options =>
            //    options.ReverseEngineerOptions = ReverseEngineerOptions.DbContextAndEntities);
        }
    }

    public class CustomCandidateNamingService : CandidateNamingService
    {
        public override string GetDependentEndCandidateNavigationPropertyName(IForeignKey foreignKey)
        {
            if (foreignKey.Properties.Count == 1 && foreignKey.Properties[0].Name.EndsWith("Id"))
                return Regex.Replace(foreignKey.Properties[0].Name, "Id$", "");

            if (foreignKey.Properties.Count == 1 && foreignKey.Properties[0].Name.EndsWith("_id"))
                return Regex.Replace(foreignKey.Properties[0].Name, "_id$", "");

            if (foreignKey.PrincipalKey.IsPrimaryKey())
                return foreignKey.PrincipalEntityType.ShortName();

            return base.GetDependentEndCandidateNavigationPropertyName(foreignKey);
        }
    }
}
