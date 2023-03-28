using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using System;
using System.Collections.Generic;
using System.IO;

namespace Cohub.WebApp
{
    public class Program
    {
#if DEBUG
        public static bool Debug { get; } = true;
#else
        public static bool Debug { get; } = false;
#endif
        public static readonly Random Random = new Random();
        public static readonly DateTimeOffset Started = DateTimeOffset.Now;
        public static bool IsDevelopment { get; private set; }
        public static long Seed => IsDevelopment ? DateTimeOffset.Now.Ticks : Started.Ticks;

        public static void Main(string[] args)
        {
            string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            IsDevelopment = envName == "Development";
            string configPath = Path.GetFullPath($"../config/appsettings.{envName}.json");
            string secretsPath = Path.GetFullPath($"../.secrets/secrets.json");

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{envName}.json", optional: true)
                .AddUserSecrets<Program>(optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddJsonFile(configPath, optional: IsDevelopment, reloadOnChange: true)
                .AddJsonFile(secretsPath, optional: true, reloadOnChange: true)
                .Build();

            LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                // `LogEventLevel` requires `using Serilog.Events;`
                //.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console();

            if (configuration.GetValue<string>("Seq:ServerUrl") is string seqServerUrl &&
                !seqServerUrl.IsNullOrWhiteSpace())
            {
                loggerConfiguration.WriteTo.Seq(seqServerUrl, apiKey: configuration.GetValue<string>("Seq:ApiKey").Nullify());
            }

            if (configuration.GetConnectionString("log_npgsql") is string serilogConnectionString &&
                !serilogConnectionString.IsNullOrWhiteSpace())
            {
                IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
                {
                    {"raise_date", new TimestampColumnWriter() },
                    {"level", new LevelColumnWriter() },
                    {"message", new RenderedMessageColumnWriter() },
                    {"message_template", new MessageTemplateColumnWriter() },
                    {"exception", new ExceptionColumnWriter() },
                    {"properties", new LogEventSerializedColumnWriter() },
                    {"props_test", new PropertiesColumnWriter() },
                };
                loggerConfiguration.WriteTo.PostgreSQL(serilogConnectionString, "logs", columnWriters, needAutoCreateTable: true);
            }

            Log.Logger = loggerConfiguration.CreateLogger();

            try
            {
                Log.Information("Starting up");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .ConfigureAppConfiguration((webHostBuilderContext, configurationBuilder) =>
                        {
                            string currentWorkingDirectory = Path.GetFullPath($".");
                            string contentRootPath = webHostBuilderContext.HostingEnvironment.ContentRootPath.TrimEnd('/', '\\');
                            if (currentWorkingDirectory != contentRootPath)
                            {
                                Log.Logger.Fatal("The '{CurrentWorkingDirectory}' current working directory and '{ContentRootPath}' hosting environment content root do not match.", currentWorkingDirectory, contentRootPath);
                            }

                            string configPath = Path.GetFullPath($"../config/appsettings.{webHostBuilderContext.HostingEnvironment.EnvironmentName}.json");
                            if (!webHostBuilderContext.HostingEnvironment.IsDevelopment() && !File.Exists(configPath))
                            {
                                Log.Logger.Fatal("The '{ConfigPath}' config file does not exist.", currentWorkingDirectory, contentRootPath);
                            }

                            configurationBuilder.AddJsonFile(configPath, optional: true, reloadOnChange: true);

                            string secretsPath = Path.GetFullPath($"../.secrets/secrets.json");
                            configurationBuilder.AddJsonFile(secretsPath, optional: true, reloadOnChange: true);
                        });
                })
                .UseSerilog();
    }
}
