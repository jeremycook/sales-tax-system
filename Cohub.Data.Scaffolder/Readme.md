# Cohub.Data.Scaffolder

## SQL Server

```cmd
# cd into this project's directory and then:
dotnet ef dbcontext scaffold "Data Source=localhost\SQLEXPRESS; Initial Catalog=anywhereusa_cohub; Integrated Security=True" Microsoft.EntityFrameworkCore.SqlServer -o ../Cohub.Data/Generated -c CohubDbContext --context-dir ../Cohub.Data/Generated/Context -n Cohub.Data --no-onconfiguring -f
```
