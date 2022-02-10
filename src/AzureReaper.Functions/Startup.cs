using System;
using AzureReaper.Functions.Factories;
using AzureReaper.Functions.Interfaces;
using AzureReaper.Functions.Provider;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(AzureReaper.Functions.Startup))]

namespace AzureReaper.Functions;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddHttpClient("AzureApiClient", configuration =>
        {
            configuration.BaseAddress = new Uri("https://management.azure.com/");
        });
        builder.Services.AddTransient<IAzureAuthProvider, AzureAuthProvider>();
    }
}