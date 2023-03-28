<Query Kind="Statements">
  <Reference Relative="..\Cohub.Data\bin\Debug\netstandard2.1\Cohub.Data.dll">&lt;MyDocuments&gt;\Repos\AnywhereUSA\Cohub.Data\bin\Debug\netstandard2.1\Cohub.Data.dll</Reference>
  <Reference Relative="..\Cohub.Data\bin\Debug\netstandard2.1\SiteKit.dll">&lt;MyDocuments&gt;\Repos\AnywhereUSA\Cohub.Data\bin\Debug\netstandard2.1\SiteKit.dll</Reference>
  <NuGetReference>CsvHelper</NuGetReference>
  <NuGetReference>NodaTime</NuGetReference>
  <Namespace>Cohub.Data</Namespace>
  <Namespace>Cohub.Data.AnywhereUSA</Namespace>
  <Namespace>Cohub.Data.Fin</Namespace>
  <Namespace>Cohub.Data.Org</Namespace>
  <Namespace>Cohub.Data.Usr</Namespace>
  <Namespace>CsvHelper</Namespace>
  <Namespace>CsvHelper.Configuration</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>NodaTime.TimeZones</Namespace>
  <Namespace>SiteKit.Info</Namespace>
  <Namespace>SiteKit.Users</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>Humanizer</Namespace>
  <RuntimeVersion>5.0</RuntimeVersion>
</Query>

// Example: Server=127.0.0.1;Port=5432;Database=anywhereusa_dev;User Id=user;Password=password");
var newPassword = Util.ReadLine("New Password", defaultValue: Util.GetPassword("anywhereusa_cohub_connection_string"));
Util.SetPassword("anywhereusa_cohub_connection_string", newPassword);
