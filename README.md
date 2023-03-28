# Sales Tax System

# Upgrading

```sh
dotnet tool install --global dotnet-outdated-tool
dotnet tool update --global dotnet-outdated-tool

cd [solution-folder]
dotnet-outdated -inc Microsoft. -inc System. -inc EntityFramework -inc EFCore -inc Npgsql -inc AngleSharp -vl Major -u
dotnet-outdated -exc Microsoft. -exc System. -exc EntityFramework -exc EFCore -exc Npgsql -exc AngleSharp -exc SoapCore -u
dotnet-outdated
```