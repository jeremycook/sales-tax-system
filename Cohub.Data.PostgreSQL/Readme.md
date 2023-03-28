# Cohub.Data.PostgreSQL Readme

```
cd ./to-solution-folder

# This is required once after cloning the repo
dotnet tool restore

# Add a migration
dotnet ef migrations add 1 -c CohubDbContext -p Cohub.Data.PostgreSQL -s Cohub.WebApp

# Apply migrations
dotnet ef database update -c CohubDbContext -s Cohub.WebApp

# Apply/revert to a specific migration
dotnet ef database update MIGRATION -c CohubDbContext -s Cohub.WebApp

# Remove last migration, the migration cannot be applied to the database
dotnet ef migrations remove -c CohubDbContext -p Cohub.Data.PostgreSQL -s Cohub.WebApp

# Revert all migrations
dotnet ef database update 0 -c CohubDbContext -s Cohub.WebApp

# Remove all migrations
rmdir .\Cohub.Data.PostgreSQL\Migrations\ -Recurse

# Start over: DELETE all migrations, add the Init migration and apply start over
dotnet ef database update 0 -c CohubDbContext -s Cohub.WebApp; dotnet ef migrations add 1 -c CohubDbContext -p Cohub.Data.PostgreSQL -s Cohub.WebApp; dotnet ef database update -c CohubDbContext -s Cohub.WebApp
```
