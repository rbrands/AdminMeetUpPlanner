﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Cosmos;
using BlazorApp.Api.Repositories;
using BlazorApp.Shared;

[assembly: FunctionsStartup(typeof(BlazorApp.Api.Startup))]
namespace BlazorApp.Api
{
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// To use a static Cosmos DB client create class for Dependency Injection.
        /// See: https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
        /// and https://towardsdatascience.com/working-with-azure-cosmos-db-in-your-azure-functions-cc4f0f98a44d
        /// and https://blog.rasmustc.com/azure-functions-dependency-injection/
        /// </summary>
        /// <param name="builder"></param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddFilter(level => true);
            });



            var config = new ConfigurationBuilder()
                           .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                           .AddEnvironmentVariables()
                           .Build();

            CosmosClient cosmosClient = new CosmosClient(config["COSMOS_DB_CONNECTION_STRING"]);
            builder.Services.AddSingleton(config);
            builder.Services.AddSingleton(cosmosClient);
            builder.Services.AddSingleton<CosmosDBRepository<TenantSettings>>();
            builder.Services.AddSingleton<CosmosDBRepository<ServerSettings>>();
            builder.Services.AddSingleton<CosmosDBRepository<ClientSettings>>();
            builder.Services.AddSingleton<CosmosDBRepository<Participant>>();
            builder.Services.AddSingleton<CosmosDBRepository<CalendarItem>>();
            builder.Services.AddSingleton<CosmosDBRepository<ExportLogItem>>();
            builder.Services.AddSingleton<CosmosDBRepository<NotificationSubscription>>();

        }
    }
}
