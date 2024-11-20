using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
using BlazorApp.Api.Repositories;
using BlazorApp.Shared;


/// <summary>
/// To use a static Cosmos DB client create class for Dependency Injection.
/// See: https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
/// and https://towardsdatascience.com/working-with-azure-cosmos-db-in-your-azure-functions-cc4f0f98a44d
/// and https://blog.rasmustc.com/azure-functions-dependency-injection/
/// </summary>
/// <param name="builder"></param>

var config = new ConfigurationBuilder()
               .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
               .AddEnvironmentVariables() // Set environment variable for local testing
               .Build();

CosmosClient cosmosClient = new CosmosClient(config["COSMOS_DB_CONNECTION_STRING"]);

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(s =>
    {
        s.AddApplicationInsightsTelemetryWorkerService();
        s.ConfigureFunctionsApplicationInsights();
        s.AddMvc().AddNewtonsoftJson();
        s.AddSingleton(config);
        s.AddSingleton(cosmosClient);
        s.AddSingleton<CosmosDBRepository<TenantSettings>>();
        s.AddSingleton<CosmosDBRepository<ServerSettings>>();
        s.AddSingleton<CosmosDBRepository<ClientSettings>>();
        s.AddSingleton<CosmosDBRepository<Participant>>();
        s.AddSingleton<CosmosDBRepository<CalendarItem>>();
        s.AddSingleton<CosmosDBRepository<ExportLogItem>>();
        s.AddSingleton<CosmosDBRepository<NotificationSubscription>>();
    }
    )
    .ConfigureLogging(logging =>
    {
        logging.Services.Configure<LoggerFilterOptions>(options =>
        {
            LoggerFilterRule defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName
                == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
            if (defaultRule is not null)
            {
                options.Rules.Remove(defaultRule);
            }
        });
     })
    .Build();

host.Run();
