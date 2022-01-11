using AzureReaper.Functions;
using AzureReaper.Functions.Interfaces;
using AzureReaper.Functions.Models;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.Extensions.Configuration;

[assembly: FunctionsStartup(typeof(AzureReaper.Functions.Startup))]

namespace AzureReaper.Functions;

public class Startup : FunctionsStartup
{
    // public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    // {
    //     FunctionsHostBuilderContext context = builder.GetContext();
    //
    //     builder.ConfigurationBuilder.AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"),
    //             optional: true, reloadOnChange: false)
    //         .AddEnvironmentVariables();
    // }
    
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddHttpClient();
        builder.Services.AddTransient<IEntityFactory, EntityFactory>();
    }
}